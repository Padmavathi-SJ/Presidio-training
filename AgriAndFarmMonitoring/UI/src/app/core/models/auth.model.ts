export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  success: boolean;
  data: {
    id: number;
    name: string;
    email: string;
    accessToken: string;
    refreshToken: string;
    farmId: number;
    farmName: string;
    role: string;  
    userType?: string; 
    accessTokenExpiresAt: string;
    refreshTokenExpiresAt: string;
  };
  errors?: string[];
}

export interface RegisterRequest {
  farmName: string;
  farmEmail: string;
  farmPhone?: string;
  farmAddress?: string;
  farmCity?: string;
  farmState?: string;
  farmCountry?: string;
  farmPostalCode?: string;
  totalLandHectares?: number;
  adminName: string;
  adminEmail: string;
  adminPassword: string;
  adminPhone?: string;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}