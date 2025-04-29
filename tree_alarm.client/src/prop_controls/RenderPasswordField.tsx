/* eslint-disable react-hooks/rules-of-hooks */

import { TextField, InputAdornment, IconButton } from "@mui/material";
import { useState } from "react";
import { IControlSelector } from "./control_selector_common";
import { Visibility, VisibilityOff } from "@mui/icons-material";

export function renderPasswordField(props: IControlSelector) {
    const [showPassword, setShowPassword] = useState(false);

    const toggleVisibility = () => setShowPassword((prev) => !prev);

    return (
        <TextField
            size="small"
            fullWidth
            type={showPassword ? 'text' : 'password'}
            id={props.prop_name}
            label={props.prop_name}
            value={props.str_val}
            onChange={props.handleChangeProp}
            InputProps={{
                endAdornment: (
                    <InputAdornment position="end">
                        <IconButton onClick={toggleVisibility} edge="end">
                            {showPassword ? <VisibilityOff /> : <Visibility />}
                        </IconButton>
                    </InputAdornment>
                )
            }} />
    );
}
