<template>
  <section v-if="quiz">
    <h2>{{ quiz.title }}</h2>
    <p>{{ quiz.description }}</p>

    <form @submit.prevent="submitQuiz">
      <div
        v-for="question in quiz.questions"
        :key="question.id"
        class="question-block"
      >
        <h4>{{ question.text }} ({{ question.points }} pts)</h4>

        <div v-for="opt in question.options" :key="opt.id">
          <label>
            <input
              type="radio"
              :name="`question_${question.id}`"
              :value="opt.id"
              v-model="answers[question.id]"
              required
            />
            {{ opt.text }}
          </label>
        </div>
      </div>

      <button type="submit" class="btn-primary">Submit</button>
    </form>

    <div v-if="result" class="mt-3">
      <h3>Your result</h3>
      <p>{{ result.earned }} / {{ result.total }} points ({{ result.percent }}%)</p>
    </div>
  </section>

  <div v-else-if="loading">Loading quiz...</div>
  <div v-else-if="error" class="alert alert-danger">{{ error }}</div>
</template>

<script setup lang="ts">
import { onMounted, ref, reactive } from 'vue';
import { useRoute } from 'vue-router';
import { getQuiz, type QuizDetail } from '../services/quizService';

const route = useRoute();
const quiz = ref<QuizDetail | null>(null);
const loading = ref(true);
const error = ref<string | null>(null);

// questionId -> selected optionId
const answers = reactive<Record<number, number | null>>({});

const result = ref<{ earned: number; total: number; percent: number } | null>(null);

onMounted(async () => {
  try {
    const id = Number(route.params.id);
    quiz.value = await getQuiz(id);

    // init answers
    quiz.value.questions.forEach(q => {
      answers[q.id] = null;
    });
  } catch (e: any) {
    console.error(e);
    error.value = 'Failed to load quiz.';
  } finally {
    loading.value = false;
  }
});

function submitQuiz() {
  if (!quiz.value) return;

  let total = 0;
  let earned = 0;

  for (const q of quiz.value.questions) {
    total += q.points;
    const selectedId = answers[q.id];
    if (!selectedId) continue;

    const option = q.options.find(o => o.id === selectedId);
    if (option && option.isCorrect) {
      earned += q.points;
    }
  }

  const percent = total > 0 ? Math.round((earned / total) * 100) : 0;
  result.value = { earned, total, percent };
}
</script>

<style scoped>
.question-block {
  border-bottom: 1px solid #eee;
  padding: 0.75rem 0;
}
.btn-primary {
  margin-top: 1rem;
}
</style>