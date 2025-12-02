// wwwroot/js/quizTake.js
import { getQuiz } from "./api.js";

async function loadQuiz() {
    const info = document.getElementById("quiz-api-info");
    if (!info) return;

    const id = info.dataset.quizId;

    try {
        const quiz = await getQuiz(id);

        // simple example: show some info loaded via API
        info.textContent = `Loaded via API: ${quiz.title} (${quiz.questions.length} questions)`;
    } catch (err) {
        console.error("Client error loading quiz via API:", err);
        alert("Could not load quiz details from the server.");
    }
}

// Run when the page is ready
document.addEventListener("DOMContentLoaded", loadQuiz);
