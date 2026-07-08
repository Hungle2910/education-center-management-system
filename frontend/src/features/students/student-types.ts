export interface ParentSummaryResponse {
  id: string;
  fullName: string;
  email?: string;
  phoneNumber: string;
  zaloLink: string;
  relationship?: string;
}

export interface StudentSummaryResponse {
  id: string;
  fullName: string;
  studentCode?: string;
  phoneNumber?: string;
  relationship?: string;
}

export interface StudentResponse {
  id: string;
  fullName: string;
  studentCode?: string;
  email?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  status: string;
  parents: ParentSummaryResponse[];
}

export interface ParentResponse {
  id: string;
  fullName: string;
  email?: string;
  phoneNumber: string;
  zaloLink: string;
  students: StudentSummaryResponse[];
}

export interface ParentLinkRequest {
  parentId: string;
  relationship?: string;
}

export interface StudentLinkRequest {
  studentId: string;
  relationship?: string;
}

export interface StudentRequest {
  fullName: string;
  studentCode?: string;
  email?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  status?: string;
  parents?: ParentLinkRequest[];
}

export interface ParentRequest {
  fullName: string;
  email?: string;
  phoneNumber: string;
  students?: StudentLinkRequest[];
}
