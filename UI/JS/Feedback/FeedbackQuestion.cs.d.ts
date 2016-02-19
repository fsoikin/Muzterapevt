declare module server {
	interface FeedbackQuestion {
		name: string;
		email: string;
		subject: string;
		text: string;
		toEmail: string;
	}
}
