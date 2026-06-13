export interface Proveedor {
  id?: number;
  nitEmpresa: string;
  nombreEmpresa: string;
  pais: string;
  tipoCarga?: 'MANUAL' | 'AUTOMATICO';
  estadoRegistro: string;
}