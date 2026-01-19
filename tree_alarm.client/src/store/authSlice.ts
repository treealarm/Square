import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";

interface LoginDto {
  username: string;
  password: string;
}

export interface AuthState {
  realm: string | null;
  token: string | null;
  refresh_token: string | null;
  loading: boolean;
  error?: string;
}

const initialState: AuthState = {
  realm: localStorage.getItem("realm"),
  token: localStorage.getItem("token"),
  refresh_token: localStorage.getItem("refresh_token"),
  loading: false,
};

export function getTokenExp(token: string): number | null {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload.exp * 1000; // ms
  } catch {
    return null;
  }
}


export const customer_login = createAsyncThunk<
  {token: string; refresh_token:string },
  LoginDto
  >("auth/customer_login", async ({ username, password }) => {
    const res = await fetch("/api/Auth/customer_login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });

  const text = await res.text();

  if (!res.ok) {
    throw new Error(text || `Login failed (${res.status})`);
  }

  const data = JSON.parse(text);
  const token = data.access_token;
  const refresh_token = data.refresh_token;
  localStorage.setItem("token", token);
  localStorage.setItem("refresh_token", refresh_token);

  return { token , refresh_token};
});

export const refreshToken = createAsyncThunk<
  { token: string; refresh_token: string },
  void
>(
  "auth/refresh",
  async () => {
    const refresh = localStorage.getItem("refresh_token");
    const res = await fetch("/api/Auth/RefreshToken", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refresh_token: refresh }),
    });
    if (!res.ok) throw new Error("Cannot refresh token");

    const data = await res.json();

    localStorage.setItem("token", data.access_token);
    localStorage.setItem("refresh_token", data.refresh_token);

    return {
      token: data.access_token,
      refresh_token: data.refresh_token
    };
  }
);


export const validateToken = createAsyncThunk<void, void>(
  "auth/validate",
  async (_, { dispatch }) => {
    const token = localStorage.getItem("token");
    if (!token) throw new Error("No token");

    const res = await fetch("/api/Auth/ValidateToken", {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (res.status === 401) {
      dispatch(logout());
      throw new Error("Token expired or invalid");
    }
  }
);


const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    logout(state) {
      state.realm = null;
      state.token = null;
      localStorage.removeItem("token");
      localStorage.removeItem("refresh_token");
    },
    restoreSession(state) {
      const savedToken = localStorage.getItem("token");
      const savedRefreshToken = localStorage.getItem("refresh_token");
      if (savedToken) {
        state.token = savedToken;
        state.refresh_token = savedRefreshToken;
      }
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(customer_login.pending, (state) => {
        state.loading = true;
        state.error = undefined;
      })
      .addCase(customer_login.fulfilled, (state, action) => {
        state.loading = false;
        state.token = action.payload.token;
        state.refresh_token = action.payload.refresh_token;
      })
      .addCase(customer_login.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message;
      });
  },
});

export const { logout, restoreSession } = authSlice.actions;
export const reducer = authSlice.reducer;
