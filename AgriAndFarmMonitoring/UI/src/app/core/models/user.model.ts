export interface User {
  id: number;
  name: string;
  email: string;
  farmId: number;
  farmName: string;
  role: 'Admin' | 'Worker' | string; 
  userType?: 'Admin' | 'Worker' | string; 
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: Date;
  refreshTokenExpiresAt: Date;
}

export interface Farm {
  id: number;
  farmName: string;
  email: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  totalLandHectares?: number;
  isActive: boolean;
  logoUrl?: string;
}