/**
 * Service layer for quiz API operations. Separates HTTP request handling from business logic.
 * Used by Vue.js components to interact with the QuizApiController on the server.
 * All methods return promises and throw errors if the API call fails.
 */
export default {
    /**
     * Fetches all quizzes from the API. Used by the quiz list page to display available quizzes.
     * Returns an array of quiz objects with id, title, and description.
     * @returns {Promise<Array>} Array of quiz objects
     */
    async getAll() {
        const res = await fetch("/api/QuizApi");
        if (!res.ok) {
            throw new Error(`Failed to fetch quizzes: ${res.status}`);
        }
        return await res.json();
    },

    /**
     * Fetches a single quiz by ID, including all its questions and answer options.
     * Used by the quiz-taking page to load the full quiz content.
     * Returns a quiz object with nested questions and options.
     * @param {number} id - The quiz ID
     * @returns {Promise<Object>} Quiz object with questions and options
     */
    async get(id) {
        const res = await fetch(`/api/QuizApi/${id}`);
        if (!res.ok) {
            throw new Error(`Failed to fetch quiz ${id}: ${res.status}`);
        }
        return await res.json();
    },

    /**
     * Creates a new quiz via the API. Used by the admin quiz creation page.
     * Sends quiz data (title, description) to the server and returns the created quiz with its generated ID.
     * @param {Object} data - Quiz data object with title and description
     * @returns {Promise<Object>} Created quiz object with quizId
     */
    async create(data) {
        const res = await fetch("/api/QuizApi", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        });

        if (!res.ok) {
            const error = await res.json().catch(() => ({ message: "Failed to create quiz" }));
            throw new Error(error.message || "Failed to create quiz");
        }
        return await res.json();
    },

    /**
     * Updates an existing quiz's title and description via the API.
     * Used by the admin quiz edit page.
     * @param {Object} data - Quiz data object with quizId, title, and description
     * @returns {Promise<void>}
     */
    async update(data) {
        const payload = {
            quizId: data.quizId,
            title: data.title,
            description: data.description
        };

        const res = await fetch(`/api/QuizApi/${data.quizId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const error = await res.json().catch(() => ({ message: "Failed to update quiz" }));
            throw new Error(error.message || "Failed to update quiz");
        }
    },

    /**
     * Deletes a quiz by ID via the API. Used by the admin quiz deletion page.
     * Permanently removes the quiz and all its questions and options from the database.
     * @param {number} id - The quiz ID to delete
     * @returns {Promise<void>}
     */
    async delete(id) {
        const res = await fetch(`/api/QuizApi/${id}`, {
            method: "DELETE"
        });

        if (!res.ok) {
            throw new Error(`Failed to delete quiz ${id}: ${res.status}`);
        }
    }
};
