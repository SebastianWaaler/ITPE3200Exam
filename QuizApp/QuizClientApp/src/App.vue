<template>
  <div class="app">
    <header class="app-header">
      <h1>QuizApp SPA</h1>
      <nav>
        <router-link to="/quizzes">Quizzes</router-link>
      </nav>
    </header>

    <main class="app-main">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
</script>

<style>
.app {
  font-family: system-ui, sans-serif;
  max-width: 900px;
  margin: 0 auto;
  padding: 1rem;
}
.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}
nav a {
  margin-left: 1rem;
}
.app-main {
  background: #fff;
  padding: 1rem;
  border-radius: 8px;
  box-shadow: 0 2px 6px rgba(0,0,0,0.05);
}
</style>