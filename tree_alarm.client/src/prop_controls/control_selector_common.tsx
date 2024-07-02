export interface IControlSelector {
  prop_name: string;
  str_val: string;
  visual_type: string | null;
  handleChangeProp: (e: any) => void;
}