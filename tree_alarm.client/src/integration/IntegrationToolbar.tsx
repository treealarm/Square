
import {
  AppBar,
  Box,
  Toolbar
} from "@mui/material";

export function IntegrationToolbar() {
  return (
    <Box sx={{ flexGrow: 1 }}>
      <AppBar sx={{ backgroundColor: '#bbbbbb' }} >
        <Toolbar variant='dense' >
          <Box
            sx={{ flexGrow: 1 }}
            display="flex"
            justifyContent="flex-start"
          >
          </Box>

          <Box
            sx={{ flexGrow: 1 }}
            display="flex"
            justifyContent="flex-end"
          >

          </Box>

          <Box
            sx={{ flexGrow: 1 }}
            display="flex"
            justifyContent="flex-end"
          >            

          </Box>          

          <Box
            display="flex"
            justifyContent="flex-end"
            alignContent="center"
          >

          </Box>

        </Toolbar>
      </AppBar>
    </Box>
  );
}