import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Proveedor } from '../models/proveedor.model';

@Injectable({
  providedIn: 'root'
})
export class ProveedorService {
  // Dirección física donde tu backend atiende peticiones con éxito (Estatus 200)
  private apiUrl = 'https://localhost:53528/api';

  constructor(private http: HttpClient) {}

  // Consume: GET /api/proveedores
  obtenerTodos(): Observable<Proveedor[]> {
    return this.http.get<Proveedor[]>(`${this.apiUrl}/proveedores`);
  }

  // Consume: POST /api/proveedores/manual
  registrarManual(proveedor: Proveedor): Observable<Proveedor> {
    return this.http.post<Proveedor>(`${this.apiUrl}/proveedores/manual`, proveedor);
  }

  // Consume: PUT /api/proveedores/{id}
  actualizarProveedor(id: number, proveedor: Proveedor): Observable<Proveedor> {
    return this.http.put<Proveedor>(`${this.apiUrl}/proveedores/${id}`, proveedor);
  }

  // Consume: POST /api/proveedores/upload-archivo (Para simular la acción del robot)
  uploadArchivoRobot(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<any>(`${this.apiUrl}/proveedores/upload-archivo`, formData);
  }
}
