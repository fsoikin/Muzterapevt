declare module server {
	interface SpecialistView {
		Id: number;
		FirstName: string;
		LastName: string;
		PatronymicName: string;
		City: string;
		Profession: string;
		ProfessionDescription: string;
		Specialization: string;
		SpecializationDescription: string;
		Resume: string;
		Email: string;
		IsEmailPublic: boolean;
		Phone: string;
		IsPhonePublic: boolean;
		Url: string;
		PhotoUrl: string;
		Organization: string;
		Countries: string[];
	}
}
