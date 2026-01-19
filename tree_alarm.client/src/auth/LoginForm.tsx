
import { useState } from "react";
import { TextField, Button, Box } from "@mui/material";
import { useAppDispatch } from "../store/configureStore";
import { customer_login } from "../store/authSlice";
import { useNavigate } from "react-router-dom";

export function LoginForm() {
  const dispatch = useAppDispatch();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  const navigate = useNavigate();

  const handleSubmit = async () => {
    try {
      const resultAction = await dispatch(
        customer_login({ username, password })
      );

      if (customer_login.rejected.match(resultAction)) {
        setError(resultAction.payload as string);
      } else {
        setError(null);
        navigate("/");
      }
    } catch (err: any) {
      setError(err.message);
    }
  };

  return (
    <Box display="flex" flexDirection="column" gap={2} maxWidth={400}>
      <TextField
        label="Username"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
      />
      <TextField
        label="Password"
        type="password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />
      {error && <div style={{ color: "red" }}>{error}</div>}
      <Button variant="contained" onClick={handleSubmit}>
        Login
      </Button>
    </Box>
  );
}
