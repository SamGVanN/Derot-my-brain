/**
 * Quiz data structure matching QuizDto from backend
 */
export interface Quiz {
    questions: Question[];
}

/**
 * Individual question matching QuestionDto from backend
 */
export interface Question {
    id: number;
    text: string;
    options?: string[]; // For MCQ
    correctOptionIndex?: number; // For MCQ (not exposed to client during quiz)
    correctAnswer?: string; // For OpenEnded (not exposed to client during quiz)
    explanation: string;
    type: 'MCQ' | 'OpenEnded';
}

/**
 * Quiz submission data matching QuizSubmissionDto from backend
 */
export interface QuizSubmission {
    answers: AnswerSubmission[];
    durationSeconds: number;
}

/**
 * Individual answer submission matching AnswerSubmissionDto from backend
 */
export interface AnswerSubmission {
    questionId: number;
    selectedOption?: string; // For MCQ
    textAnswer?: string; // For OpenEnded
}

/**
 * Quiz evaluation result matching QuizResultDto from backend
 */
export interface QuizResult {
    totalQuestions: number;
    correctAnswers: number;
    scorePercentage: number;
    results: QuestionResult[];
}

/**
 * Individual question result matching QuestionResultDto from backend
 */
export interface QuestionResult {
    questionId: number;
    isCorrect: boolean;
    userAnswer?: string;
    correctAnswer?: string;
    explanation: string;
    semanticScore?: number; // For OpenEnded questions (0.0-1.0)
}
