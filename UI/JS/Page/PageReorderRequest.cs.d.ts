declare module server {
	interface PageReorderRequest {
		ParentId: number;
		Children: number[];
	}
}
