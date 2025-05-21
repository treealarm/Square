/* eslint-disable no-unused-vars */
import { Autocomplete, TextField, Chip } from '@mui/material';

interface EnumListInputProps {
  label: string;
  value: string[]; // массив строк
  onChange: (value: string[]) => void;
}

export function EnumListInput({ label, value, onChange }: EnumListInputProps) {
  return (
    <Autocomplete
      multiple
      freeSolo
      options={[]} // можно передать список подсказок
      value={value}
      onChange={(event, newValue) => {
        const cleaned = Array.from(
          new Set(newValue.map((v) => v.trim()).filter((v) => v !== ''))
        );
        onChange(cleaned);
      }}
      renderTags={(tagValue, getTagProps) =>
        tagValue.map((option, index) => (
          <Chip
            key={index}
            variant="outlined"
            label={option}
            {...getTagProps({ index })}
          />
        ))
      }
      renderInput={(params) => (
        <TextField
          {...params}
          variant="outlined"
          label={label}
          placeholder={value.length === 0 ? 'Add...' : ''}
        />
      )}
    />
  );
}
