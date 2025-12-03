import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:5001/api/quiz', // adjust port if needed
  withCredentials: true
});

export interface QuizSummary {
  quizId: number;
  title: string;
  description: string | null;
}

export interface QuizOption {
  id: number;
  text: string;
  isCorrect: boolean;
}

export interface QuizQuestion {
  id: number;
  text: string;
  points: number;
  options: QuizOption[];
}

export interface QuizDetail {
  quizId: number;
  title: string;
  description: string | null;
  questions: QuizQuestion[];
}

export async function getQuizzes(): Promise<QuizSummary[]> {
  const res = await api.get<QuizSummary[]>('/');
  return res.data;
}

export async function getQuiz(id: number): Promise<QuizDetail> {
  const res = await api.get<QuizDetail>(`/${id}`);
  return res.data;
}