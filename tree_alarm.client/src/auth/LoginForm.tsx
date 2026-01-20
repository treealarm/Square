import { useEffect, useState } from "react";
import {
  TextField,
  Button,
  Box,
  Card,
  CardContent,
  Typography,
} from "@mui/material";
import { useAppDispatch, useAppSelector } from "../store/configureStore";
import { customer_login } from "../store/authSlice";
import { useNavigate } from "react-router-dom";

export function LoginForm() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const { error } = useAppSelector(s => s.authStates);
  const token = useAppSelector((s) => s.authStates.token);

  useEffect(() => {
    if (token) {
      navigate("/", { replace: true });
    }
  }, [token, navigate]);

  const handleSubmit = async () => {
    dispatch(
      customer_login({ username, password })
    );
  };


  return (
    <Box
      minHeight="100vh"
      display="flex"
      alignItems="center"
      justifyContent="center"
      bgcolor="background.default"
    >
      <Card
        elevation={3}
        sx={{
          width: 360,
          borderRadius: 2,
        }}
      >
        <CardContent sx={{ p: 4 }}>
          <Typography
            variant="h5"
            textAlign="center"
            mb={3}
            fontWeight={500}
          >
            Sign in
          </Typography>

          <Box display="flex" flexDirection="column" gap={2}>
            <TextField
              label="Username"
              fullWidth
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              InputLabelProps={{ shrink: true }}
            />

            <TextField
              label="Password"
              type="password"
              fullWidth
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              InputLabelProps={{ shrink: true }}
            />

            {error && (
              <Typography color="error" variant="body2">
                {error}
              </Typography>
            )}

            <Button
              variant="contained"
              size="large"
              fullWidth
              onClick={handleSubmit}
            >
              Login
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}
