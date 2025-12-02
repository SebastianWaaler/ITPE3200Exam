// wwwroot/js/api.js

export async function getQuiz(id) {
    const response = await fetch(`/api/quizapi/${id}`);

    if (!response.ok) {
        throw new Error(`Failed to load quiz ${id}: ${response.status}`);
    }

    return await response.json();
}
