<template>
  <section>
    <h2>Available Quizzes</h2>

    <div v-if="loading">Loading quizzes...</div>
    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>
    <ul v-else class="quiz-list">
      <li v-for="quiz in filteredQuizzes" :key="quiz.quizId" class="quiz-item">
        <h3>{{ quiz.title }}</h3>
        <p>{{ quiz.description }}</p>
        <router-link :to="`/quizzes/${quiz.quizId}`" class="btn">
          Take Quiz
        </router-link>
      </li>
    </ul>

    <div class="mt-3">
      <label>
        Filter by title:
        <input v-model="filter" placeholder="Search..." />
      </label>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { getQuizzes, type QuizSummary } from '../services/quizService';

const quizzes = ref<QuizSummary[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);
const filter = ref('');

const filteredQuizzes = computed(() => {
  if (!filter.value) return quizzes.value;
  const term = filter.value.toLowerCase();
  return quizzes.value.filter(q => q.title.toLowerCase().includes(term));
});

onMounted(async () => {
  try {
    quizzes.value = await getQuizzes();
  } catch (e: any) {
    console.error(e);
    error.value = 'Failed to load quizzes.';
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.quiz-list {
  list-style: none;
  padding: 0;
}
.quiz-item {
  border-bottom: 1px solid #eee;
  padding: 0.5rem 0;
}
.btn {
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 4px;
  border: 1px solid #0077cc;
  color: #0077cc;
  text-decoration: none;
}
.btn:hover {
  background: #0077cc;
  color: white;
}
</style>