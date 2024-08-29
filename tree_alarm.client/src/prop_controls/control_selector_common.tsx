export interface IControlSelector {
  prop_name: string;
  str_val: string;
  visual_type: string | null;
  // eslint-disable-next-line no-unused-vars
  handleChangeProp: (e: any) => void;
}