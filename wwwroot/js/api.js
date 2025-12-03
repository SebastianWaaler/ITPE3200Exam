/**
 * Fetches a single quiz by ID from the API, including all questions and answer options.
 * Used by the quiz-taking page to load quiz content.
 * Note: This uses a different endpoint path (/api/quizapi) than QuizService.js (/api/QuizApi).
 * @param {number} id - The quiz ID to fetch
 * @returns {Promise<Object>} Quiz object with questions and options
 * @throws {Error} If the API call fails
 */
export async function getQuiz(id) {
    const response = await fetch(`/api/quizapi/${id}`);

    if (!response.ok) {
        throw new Error(`Failed to load quiz ${id}: ${response.status}`);
    }

    return await response.json();
}
