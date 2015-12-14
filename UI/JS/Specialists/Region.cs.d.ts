declare module server {
	interface Region {
		Id: number;
		Name: string;
		children: server.Region[];
	}
}
