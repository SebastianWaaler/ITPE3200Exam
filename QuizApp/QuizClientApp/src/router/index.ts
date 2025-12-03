import { createRouter, createWebHistory } from 'vue-router';
import QuizListView from '../views/QuizListView.vue';
import QuizTakeView from '../views/QuizTakeView.vue';

const routes = [
  { path: '/', redirect: '/quizzes' },
  { path: '/quizzes', name: 'QuizList', component: QuizListView },
  { path: '/quizzes/:id', name: 'QuizTake', component: QuizTakeView, props: true }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;