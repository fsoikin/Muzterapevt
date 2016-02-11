declare module server {
	interface SpecialistRegistrationRequest {
		firstName: string;
		lastName: string;
		patronymicName: string;
		contactEmail: string;
		publicEmail: string;
		publicPhone: string;
		url: string;
		photo: number;
		organization: string;
		resume: string;
		city: string;
		regions: number[];
		professions: number[];
		professionDescription: string;
		specializations: number[];
		specializationDescription: string;
		experience: number;
		experienceDescription: string;
		formalEducation: string;
		musicTherapyEducation: string;
	}
}
