declare module server {
	interface SpecialistRegistrationRequest {
		FirstName: string;
		LastName: string;
		PatronymicName: string;
		City: string;
		ProfessionDescription: string;
		SpecializationDescription: string;
		Resume: string;
		Email: string;
		IsEmailPublic: boolean;
		Phone: string;
		IsPhonePublic: boolean;
		Url: string;
		Photo: number;
		Organization: string;
		Countries: string[];
		Profession: number;
		Specialization: number;
	}
}
