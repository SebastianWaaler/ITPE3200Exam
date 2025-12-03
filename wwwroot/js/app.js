/**
 * Main Single Page Application entry point.
 * Sets up Vue Router and the root Vue application.
 * All navigation happens client-side without page reloads.
 * 
 * Note: This uses Vue Router from CDN, so we access it via the global VueRouter object.
 */

// QuizService is already loaded globally or we'll use fetch directly

// Vue Router setup - using global VueRouter from CDN
const { createRouter, createWebHistory } = VueRouter;
const { createApp } = Vue;

// QuizService helper functions (since we can't use ES6 modules with CDN)
const QuizService = {
    async getAll() {
        const res = await fetch("/api/QuizApi");
        if (!res.ok) throw new Error(`Failed to fetch quizzes: ${res.status}`);
        return await res.json();
    },
    async get(id) {
        const res = await fetch(`/api/QuizApi/${id}`);
        if (!res.ok) throw new Error(`Failed to fetch quiz ${id}: ${res.status}`);
        return await res.json();
    },
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
    async delete(id) {
        const res = await fetch(`/api/QuizApi/${id}`, {
            method: "DELETE"
        });
        if (!res.ok) throw new Error(`Failed to delete quiz ${id}: ${res.status}`);
    }
};

// Define routes for the SPA
const routes = [
    {
        path: '/',
        name: 'QuizList',
        component: {
            template: '#quiz-list-template',
            data() {
                return {
                    quizzes: [],
                    filteredQuizzes: [],
                    searchQuery: "",
                    loading: true,
                    error: null,
                    isAdmin: false
                };
            },
            async mounted() {
                await this.loadUserInfo();
                await this.loadQuizzes();
            },
            methods: {
                async loadUserInfo() {
                    try {
                        const response = await fetch('/api/UserInfo');
                        if (response.ok) {
                            const userInfo = await response.json();
                            this.isAdmin = userInfo.isAdmin || false;
                        }
                    } catch (err) {
                        console.error('Failed to load user info:', err);
                    }
                },
                async loadQuizzes() {
                    try {
                        this.loading = true;
                        this.quizzes = await QuizService.getAll();
                        this.filteredQuizzes = this.quizzes;
                    } catch (err) {
                        console.error('Failed to load quizzes:', err);
                        this.error = "Failed to load quizzes.";
                    } finally {
                        this.loading = false;
                    }
                },
                filterQuizzes() {
                    const query = this.searchQuery.toLowerCase().trim();
                    if (!query) {
                        this.filteredQuizzes = this.quizzes;
                        return;
                    }
                    this.filteredQuizzes = this.quizzes.filter(quiz => {
                        const titleMatch = quiz.title?.toLowerCase().includes(query) || false;
                        const descMatch = quiz.description?.toLowerCase().includes(query) || false;
                        return titleMatch || descMatch;
                    });
                },
                navigateTo(path) {
                    this.$router.push(path);
                }
            }
        }
    },
    {
        path: '/quiz/create',
        name: 'QuizCreate',
        component: {
            template: '#quiz-create-template',
            data() {
                return {
                    title: "",
                    description: "",
                    errors: [],
                    saving: false
                };
            },
            methods: {
                validate() {
                    this.errors = [];
                    if (!this.title || this.title.trim() === "") {
                        this.errors.push("Title is required");
                    } else if (this.title.length > 200) {
                        this.errors.push("Title must be 200 characters or less");
                    }
                    return this.errors.length === 0;
                },
                async saveQuiz() {
                    this.errors = [];
                    if (!this.validate()) {
                        return;
                    }
                    this.saving = true;
                    try {
                        const created = await QuizService.create({
                            title: this.title.trim(),
                            description: this.description.trim()
                        });
                        this.$router.push(`/quiz/details/${created.quizId}`);
                    } catch (err) {
                        console.error("Error creating quiz:", err);
                        this.errors.push("Failed to create quiz. Please try again.");
                    } finally {
                        this.saving = false;
                    }
                },
                cancel() {
                    this.$router.push('/');
                }
            }
        }
    },
    {
        path: '/quiz/edit/:id',
        name: 'QuizEdit',
        component: {
            template: '#quiz-edit-template',
            data() {
                return {
                    quizId: null,
                    title: "",
                    description: "",
                    errors: [],
                    saving: false,
                    loading: true
                };
            },
            async mounted() {
                this.quizId = parseInt(this.$route.params.id);
                await this.loadQuiz();
            },
            methods: {
                async loadQuiz() {
                    try {
                        this.loading = true;
                        const quiz = await QuizService.get(this.quizId);
                        this.title = quiz.title;
                        this.description = quiz.description || "";
                    } catch (err) {
                        console.error("Error loading quiz:", err);
                        this.errors.push("Failed to load quiz.");
                    } finally {
                        this.loading = false;
                    }
                },
                validate() {
                    this.errors = [];
                    if (!this.title || this.title.trim() === "") {
                        this.errors.push("Title is required");
                    } else if (this.title.length > 200) {
                        this.errors.push("Title must be 200 characters or less");
                    }
                    return this.errors.length === 0;
                },
                async updateQuiz() {
                    this.errors = [];
                    if (!this.validate()) {
                        return;
                    }
                    this.saving = true;
                    try {
                        await QuizService.update({
                            quizId: this.quizId,
                            title: this.title.trim(),
                            description: this.description.trim()
                        });
                        this.$router.push('/');
                    } catch (err) {
                        console.error("Error updating quiz:", err);
                        this.errors.push("Failed to update quiz. Please try again.");
                    } finally {
                        this.saving = false;
                    }
                },
                cancel() {
                    this.$router.push('/');
                }
            }
        }
    },
    {
        path: '/quiz/details/:id',
        name: 'QuizDetails',
        component: {
            template: '#quiz-details-template',
            data() {
                return {
                    quiz: null,
                    loading: true,
                    error: null
                };
            },
            async mounted() {
                await this.loadQuiz();
            },
            methods: {
                async loadQuiz() {
                    try {
                        this.loading = true;
                        const quizId = parseInt(this.$route.params.id);
                        this.quiz = await QuizService.get(quizId);
                    } catch (err) {
                        console.error("Error loading quiz:", err);
                        this.error = "Failed to load quiz.";
                    } finally {
                        this.loading = false;
                    }
                },
                navigateTo(path) {
                    this.$router.push(path);
                }
            }
        }
    },
    {
        path: '/quiz/take/:id',
        name: 'QuizTake',
        component: {
            template: '#quiz-take-template',
            data() {
                return {
                    quizId: null,
                    quiz: null,
                    loading: true,
                    error: null,
                    answers: {}
                };
            },
            async mounted() {
                this.quizId = parseInt(this.$route.params.id);
                await this.loadQuiz();
            },
            methods: {
                async loadQuiz() {
                    try {
                        this.loading = true;
                        const response = await fetch(`/api/QuizApi/${this.quizId}`);
                        if (!response.ok) {
                            if (response.status === 401 || response.status === 403) {
                                this.error = "You must be logged in to take this quiz.";
                            } else if (response.status === 404) {
                                this.error = "Quiz not found.";
                            } else {
                                this.error = "Failed to load quiz.";
                            }
                            return;
                        }
                        this.quiz = await response.json();
                    } catch (err) {
                        console.error(err);
                        this.error = "Could not load quiz. Please try again.";
                    } finally {
                        this.loading = false;
                    }
                },
                selectAnswer(questionId, optionId) {
                    this.answers[questionId] = optionId;
                },
                async submitQuiz() {
                    try {
                        const formData = new FormData();
                        formData.append('QuizId', this.quizId);
                        Object.keys(this.answers).forEach(questionId => {
                            formData.append(`question_${questionId}`, this.answers[questionId]);
                        });

                        const response = await fetch('/Quiz/Submit', {
                            method: 'POST',
                            body: formData,
                            headers: {
                                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                            }
                        });

                        if (response.ok) {
                            const result = await response.json();
                            this.$router.push({
                                name: 'QuizResult',
                                params: { quizId: this.quizId },
                                query: { 
                                    earned: result.earnedPoints, 
                                    total: result.totalPoints,
                                    title: result.quizTitle
                                }
                            });
                        } else {
                            this.error = "Failed to submit quiz.";
                        }
                    } catch (err) {
                        console.error("Error submitting quiz:", err);
                        this.error = "Failed to submit quiz. Please try again.";
                    }
                }
            }
        }
    },
    {
        path: '/quiz/result/:quizId',
        name: 'QuizResult',
        component: {
            template: '#quiz-result-template',
            data() {
                return {
                    result: {
                        title: "",
                        earned: 0,
                        total: 0
                    }
                };
            },
            mounted() {
                this.result.title = this.$route.query.title || "";
                this.result.earned = parseInt(this.$route.query.earned) || 0;
                this.result.total = parseInt(this.$route.query.total) || 0;
            },
            computed: {
                percentage() {
                    if (this.result.total === 0) return 0;
                    return Math.round((this.result.earned / this.result.total) * 100);
                }
            }
        }
    },
    {
        path: '/quiz/delete/:id',
        name: 'QuizDelete',
        component: {
            template: '#quiz-delete-template',
            data() {
                return {
                    quizId: null,
                    title: "",
                    loading: true
                };
            },
            async mounted() {
                this.quizId = parseInt(this.$route.params.id);
                await this.loadQuiz();
            },
            methods: {
                async loadQuiz() {
                    try {
                        this.loading = true;
                        const quiz = await QuizService.get(this.quizId);
                        this.title = quiz.title;
                    } catch (err) {
                        console.error("Error loading quiz:", err);
                    } finally {
                        this.loading = false;
                    }
                },
                async deleteQuiz() {
                    try {
                        await QuizService.delete(this.quizId);
                        this.$router.push('/');
                    } catch (err) {
                        console.error("Error deleting quiz:", err);
                        alert("Failed to delete quiz.");
                    }
                },
                cancel() {
                    this.$router.push('/');
                }
            }
        }
    },
    {
        path: '/questions/create',
        name: 'QuestionCreate',
        component: {
            template: '#question-create-template',
            data() {
                return {
                    quizId: null,
                    quizTitle: "",
                    text: "",
                    points: 1,
                    options: [
                        { text: "", isCorrect: false },
                        { text: "", isCorrect: false }
                    ],
                    correctIndex: -1,
                    errors: [],
                    loading: true
                };
            },
            async mounted() {
                this.quizId = parseInt(this.$route.query.quizId);
                await this.loadQuiz();
            },
            methods: {
                async loadQuiz() {
                    try {
                        this.loading = true;
                        const quiz = await QuizService.get(this.quizId);
                        this.quizTitle = quiz.title;
                    } catch (err) {
                        console.error("Error loading quiz:", err);
                        this.errors.push("Failed to load quiz.");
                    } finally {
                        this.loading = false;
                    }
                },
                addOption() {
                    this.options.push({ text: "", isCorrect: false });
                },
                removeOption(index) {
                    this.options.splice(index, 1);
                    if (this.correctIndex === index) {
                        this.correctIndex = -1;
                    } else if (this.correctIndex > index) {
                        this.correctIndex--;
                    }
                },
                async saveQuestion() {
                    this.errors = [];
                    const cleanedOptions = this.options.filter(o => o.text.trim() !== "");
                    
                    if (cleanedOptions.length < 2) {
                        this.errors.push("At least two options required.");
                    }
                    if (this.correctIndex < 0 || this.correctIndex >= this.options.length) {
                        this.errors.push("Select a correct answer.");
                    }
                    if (!this.text.trim()) {
                        this.errors.push("Question text is required.");
                    }
                    
                    if (this.errors.length > 0) {
                        return;
                    }

                    try {
                        const formData = new FormData();
                        formData.append('QuizId', this.quizId);
                        formData.append('Text', this.text);
                        formData.append('Points', this.points);
                        formData.append('CorrectIndex', this.correctIndex);
                        this.options.forEach((opt, idx) => {
                            formData.append(`Options[${idx}].Text`, opt.text);
                        });

                        const response = await fetch('/Questions/Create', {
                            method: 'POST',
                            body: formData,
                            headers: {
                                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                            }
                        });

                        if (response.ok) {
                            this.$router.push(`/quiz/details/${this.quizId}`);
                        } else {
                            this.errors.push("Failed to create question.");
                        }
                    } catch (err) {
                        console.error("Error creating question:", err);
                        this.errors.push("Failed to create question.");
                    }
                },
                cancel() {
                    this.$router.push(`/quiz/details/${this.quizId}`);
                }
            }
        }
    },
    {
        path: '/questions/edit/:id',
        name: 'QuestionEdit',
        component: {
            template: '#question-edit-template',
            data() {
                return {
                    questionId: null,
                    quizId: null,
                    text: "",
                    points: 1,
                    options: [],
                    correctIndex: -1,
                    errors: [],
                    loading: true
                };
            },
            async mounted() {
                this.questionId = parseInt(this.$route.params.id);
                await this.loadQuestion();
            },
            methods: {
                async loadQuestion() {
                    try {
                        this.loading = true;
                        const response = await fetch(`/api/QuestionApi/${this.questionId}`);
                        if (!response.ok) throw new Error('Failed to load question');
                        const question = await response.json();
                        this.quizId = question.quizId;
                        this.text = question.text;
                        this.points = question.points;
                        this.options = question.options.map(o => ({
                            id: o.id,
                            text: o.text,
                            isCorrect: o.isCorrect
                        }));
                        this.correctIndex = this.options.findIndex(o => o.isCorrect);
                    } catch (err) {
                        console.error("Error loading question:", err);
                        this.errors.push("Failed to load question.");
                    } finally {
                        this.loading = false;
                    }
                },
                addOption() {
                    this.options.push({ id: 0, text: "", isCorrect: false });
                },
                removeOption(index) {
                    this.options.splice(index, 1);
                    if (this.correctIndex === index) {
                        this.correctIndex = -1;
                    } else if (this.correctIndex > index) {
                        this.correctIndex--;
                    }
                },
                async saveQuestion() {
                    this.errors = [];
                    const cleanedOptions = this.options.filter(o => o.text.trim() !== "");
                    
                    if (cleanedOptions.length < 2) {
                        this.errors.push("At least two options required.");
                    }
                    if (this.correctIndex < 0 || this.correctIndex >= this.options.length) {
                        this.errors.push("Select a correct answer.");
                    }
                    
                    if (this.errors.length > 0) {
                        return;
                    }

                    try {
                        const formData = new FormData();
                        formData.append('Id', this.questionId);
                        formData.append('QuizId', this.quizId);
                        formData.append('Text', this.text);
                        formData.append('Points', this.points);
                        formData.append('CorrectIndex', this.correctIndex);
                        this.options.forEach((opt, idx) => {
                            if (opt.id) formData.append(`Options[${idx}].Id`, opt.id);
                            formData.append(`Options[${idx}].Text`, opt.text);
                        });

                        const response = await fetch(`/Questions/Edit/${this.questionId}`, {
                            method: 'POST',
                            body: formData,
                            headers: {
                                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                            }
                        });

                        if (response.ok) {
                            this.$router.push(`/quiz/details/${this.quizId}`);
                        } else {
                            this.errors.push("Failed to update question.");
                        }
                    } catch (err) {
                        console.error("Error updating question:", err);
                        this.errors.push("Failed to update question.");
                    }
                },
                cancel() {
                    this.$router.push(`/quiz/details/${this.quizId}`);
                }
            },
            watch: {
                correctIndex(newVal) {
                    this.options.forEach((o, i) => {
                        o.isCorrect = (i === newVal);
                    });
                }
            }
        }
    },
    {
        path: '/questions/delete/:id',
        name: 'QuestionDelete',
        component: {
            template: '#question-delete-template',
            data() {
                return {
                    questionId: null,
                    question: null,
                    loading: true
                };
            },
            async mounted() {
                this.questionId = parseInt(this.$route.params.id);
                await this.loadQuestion();
            },
            methods: {
                async loadQuestion() {
                    try {
                        this.loading = true;
                        const response = await fetch(`/api/QuestionApi/${this.questionId}`);
                        if (!response.ok) throw new Error('Failed to load question');
                        this.question = await response.json();
                    } catch (err) {
                        console.error("Error loading question:", err);
                    } finally {
                        this.loading = false;
                    }
                },
                async deleteQuestion() {
                    try {
                        const formData = new FormData();
                        formData.append('Id', this.questionId);
                        formData.append('QuizId', this.question.quizId);

                        const response = await fetch(`/Questions/Delete/${this.questionId}`, {
                            method: 'POST',
                            body: formData,
                            headers: {
                                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                            }
                        });

                        if (response.ok) {
                            this.$router.push(`/quiz/details/${this.question.quizId}`);
                        } else {
                            alert("Failed to delete question.");
                        }
                    } catch (err) {
                        console.error("Error deleting question:", err);
                        alert("Failed to delete question.");
                    }
                },
                cancel() {
                    this.$router.push(`/quiz/details/${this.question.quizId}`);
                }
            }
        }
    }
];

// Create router
const router = createRouter({
    history: createWebHistory(),
    routes
});

// Create and mount the app
const app = createApp({
    data() {
        return {
            user: null,
            isAdmin: false
        };
    },
    async mounted() {
        await this.loadUserInfo();
    },
    methods: {
        async loadUserInfo() {
            try {
                const response = await fetch('/api/UserInfo');
                if (response.ok) {
                    const userInfo = await response.json();
                    this.user = userInfo;
                    this.isAdmin = userInfo.isAdmin || false;
                }
            } catch (err) {
                console.error('Failed to load user info:', err);
            }
        },
        logout() {
            const form = document.createElement('form');
            form.method = 'POST';
            form.action = '/Identity/Account/Logout';
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = '__RequestVerificationToken';
                input.value = token;
                form.appendChild(input);
            }
            document.body.appendChild(form);
            form.submit();
        }
    }
});

app.use(router);
app.mount('#app');

