/* eslint-disable no-unused-vars */
/* eslint-disable react/jsx-key */
import { Autocomplete, TextField, Chip } from '@mui/material';

interface EnumListInputProps {
  label: string;
  value: string[];
  onChange: (value: string[]) => void;
}

export function EnumListInput({ label, value, onChange }: EnumListInputProps) {
  return (
    <Autocomplete
      multiple
      freeSolo
      options={[]} // можно передать список подсказок, если хочешь
      value={value}
      onChange={(event, newValue) => {
        onChange(newValue);
      }}
      renderTags={(tagValue, getTagProps) =>
        tagValue.map((option, index) => (
          <Chip
            variant="outlined"
            label={option}
            {...getTagProps({ index })}
          />
        ))
      }
      renderInput={(params) => (
        <TextField {...params} variant="outlined" label={label} placeholder="Добавить..." />
      )}
    />
  );
}
