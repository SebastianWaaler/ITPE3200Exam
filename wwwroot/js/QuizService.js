export default {
    async getAll() {
        const res = await fetch("/api/QuizApi");
        return await res.json();
    },

    async get(id) {
        const res = await fetch(`/api/QuizApi/${id}`);
        return await res.json();
    },

    async create(data) {
        const res = await fetch("/api/QuizApi", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        });

        if (!res.ok) throw new Error("Failed to create quiz");
        return await res.json();
    },

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

        if (!res.ok) throw new Error("Failed to update quiz");
    },

    async delete(id) {
        const res = await fetch(`/api/QuizApi/${id}`, {
            method: "DELETE"
        });

        if (!res.ok) throw new Error("Failed to delete quiz");
    }
};
