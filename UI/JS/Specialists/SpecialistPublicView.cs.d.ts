declare module server {
	interface SpecialistPublicView {
		id: number;
		firstName: string;
		lastName: string;
		patronymicName: string;
		profession: string;
		professionDescription: string;
		specialization: string;
		specializationDescription: string;
		experience: string;
		experienceDescription: string;
		formalEducation: string;
		musicTherapyEducation: string;
		resume: string;
		email: string;
		phone: string;
		url: string;
		photoUrl: string;
		organization: string;
		city: string;
		regions: string[];
	}
}
