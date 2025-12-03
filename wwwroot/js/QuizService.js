/**
 * API Service Layer for Quiz operations.
 * Separates HTTP request handling from business logic on the client-side.
 * This service handles all communication with the QuizApiController.
 */
export default {
    /**
     * Fetches all quizzes from the API.
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
     * Fetches a single quiz by ID including questions and options.
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
     * Creates a new quiz via the API.
     * @param {Object} data - Quiz data (title, description)
     * @returns {Promise<Object>} Created quiz object
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
     * Updates an existing quiz via the API.
     * @param {Object} data - Quiz data (quizId, title, description)
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
     * Deletes a quiz by ID via the API.
     * @param {number} id - The quiz ID to delete
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
