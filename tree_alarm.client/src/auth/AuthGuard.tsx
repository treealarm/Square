
import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../store/configureStore";

import { validateToken, refreshToken, logout } from "../store/authSlice";
import { useNavigate } from "react-router-dom";
import { getTokenExp } from "../store/authSlice";

export function AuthGuard() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { token } = useAppSelector((s) => s.authStates);

  useEffect(() => {
    if (!token) return;

    // 1) Проверить токен при старте
    dispatch(validateToken())
      .unwrap()
      .catch(() => {
        dispatch(logout());
        navigate("/login");
      });

    // 2) Авто-refresh токена
    const exp = getTokenExp(token);
    if (!exp) return;

    const now = Date.now();
    const msToExpire = exp - now;

    // refresh за 30 секунд до конца
    const refreshAt = msToExpire - 30_000;

    if (refreshAt <= 0) {
      dispatch(refreshToken())
        .unwrap()
        .catch(() => {
          dispatch(logout());
          navigate("/login");
        });
      return;
    }

    const timer = setTimeout(() => {
      dispatch(refreshToken())
        .unwrap()
        .catch(() => {
          dispatch(logout());
          navigate("/login");
        });
    }, refreshAt);

    return () => clearTimeout(timer);
  }, [token, dispatch, navigate]);

  return null; // компонент ничего не рисует
}
