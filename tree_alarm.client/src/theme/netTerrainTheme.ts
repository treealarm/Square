import { alpha, createTheme } from '@mui/material'
// Registers MuiPickersDay/MuiYearCalendar/etc. on the `components` theme key below.
import type {} from '@mui/x-date-pickers/themeAugmentation'

/**
 * MUI needs real hex/rgb colors here (not CSS `var()`): it runs contrast and color-mix in JS.
 * Keep values aligned with `:root` in App.css when you change the design tokens.
 */
const primaryMain = '#c21931'
const primaryDark = '#9a1426'
const primaryLight = '#e84d63'

const panel = {
  paper: '#4a4949',
  default: '#3c3c3c',
  elevated: '#434343',
  border: '#333333',
} as const

export const netTerrainTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: primaryMain,
      dark: primaryDark,
      light: primaryLight,
      contrastText: '#ffffff',
    },
    secondary: {
      main: '#646363',
      light: '#808080',
      dark: '#565656',
      contrastText: '#ffffff',
    },
    error: {
      main: '#ef493f',
    },
    background: {
      default: panel.default,
      paper: panel.paper,
    },
    text: {
      primary: '#fcfcfc',
      secondary: '#c1c1c1',
      disabled: '#808080',
    },
    divider: panel.border,
    action: {
      active: '#ffffff',
      hover: alpha('#ffffff', 0.08),
      selected: alpha(primaryMain, 0.18),
      disabled: '#808080',
      disabledBackground: alpha('#808080', 0.24),
    },
  },
  typography: {
    fontFamily: '"Roboto", "Tahoma", "Geneva", "Arial", "sans-serif"',
  },
  shape: {
    borderRadius: 8,
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          scrollbarColor: `${alpha('#eeeeee', 0.35)} ${panel.elevated}`,
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
        },
        outlined: {
          borderColor: panel.border,
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          border: `1px solid ${panel.border}`,
          backgroundImage: 'none',
        },
      },
    },
    MuiTabs: {
      styleOverrides: {
        indicator: {
          backgroundColor: primaryMain,
          height: 3,
        },
      },
    },
    MuiTab: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 500,
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        containedPrimary: {
          fontWeight: 500,
          '&:hover': {
            backgroundColor: primaryDark,
          },
        },
      },
      defaultProps: {
        disableElevation: true,
      },
    },
    MuiToggleButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          '&.Mui-selected': {
            backgroundColor: alpha(primaryMain, 0.35),
            '&:hover': {
              backgroundColor: alpha(primaryMain, 0.45),
            },
          },
        },
      },
    },
    MuiTextField: {
      defaultProps: {
        variant: 'outlined',
        size: 'small',
      },
    },
    MuiInputLabel: {
      defaultProps: {
        shrink: true,
      },
    },
    MuiFormControl: {
      defaultProps: {
        // Adds marginTop: 8px so floating labels are never clipped
        // when a TextField is the first item in an overflow container.
        margin: 'dense',
      },
    },
    MuiOutlinedInput: {
      defaultProps: {
        // Keep the notch (gap in the border) in sync with the always-shrunk label.
        notched: true,
      },
      styleOverrides: {
        root: {
          backgroundColor: panel.elevated,
          '& fieldset': {
            borderColor: panel.border,
          },
          '&:hover fieldset': {
            borderColor: alpha('#ffffff', 0.25),
          },
          '&.Mui-focused fieldset': {
            borderColor: primaryMain,
          },
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: panel.paper,
          borderBottom: `1px solid ${panel.border}`,
        },
      },
    },
    // Compact the archive jump-to-date calendar (RecordingsList) so it doesn't
    // dwarf the rest of the panel; only date picker usage in the app.
    MuiPickersDay: {
      styleOverrides: {
        root: {
          width: 28,
          height: 28,
          fontSize: '0.7rem',
          margin: '1px',
        },
      },
    },
    MuiYearCalendar: {
      styleOverrides: {
        root: {
          '& .MuiYearCalendar-button': {
            fontSize: '0.75rem',
            padding: '4px 8px',
          },
        },
      },
    },
    MuiMonthCalendar: {
      styleOverrides: {
        root: {
          '& .MuiMonthCalendar-button': {
            fontSize: '0.75rem',
            padding: '4px 8px',
          },
        },
      },
    },
    MuiPickersCalendarHeader: {
      styleOverrides: {
        root: {
          minHeight: 32,
          paddingLeft: 8,
          paddingRight: 4,
        },
        label: {
          fontSize: '0.8rem',
        },
      },
    },
    MuiDayCalendar: {
      styleOverrides: {
        weekDayLabel: {
          fontSize: '0.7rem',
        },
      },
    },
  },
})
