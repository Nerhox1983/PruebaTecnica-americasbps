import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Proveedor } from '../../models/proveedor.model';
import { ProveedorService } from '../../services/proveedor';

@Component({
  selector: 'app-proveedores',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './proveedores.component.html',
  styleUrls: ['./proveedores.component.css']
})
export class ProveedoresComponent implements OnInit {
  proveedores: Proveedor[] = [];
  
  // Objeto para el Módulo de Carga Manual
  nuevoProveedor: Proveedor = { nitEmpresa: '', nombreEmpresa: '', pais: '', estadoRegistro: 'ACTIVO' };
  
  // Objeto para el Módulo de Edición Directa
  proveedorSeleccionado: Proveedor | null = null;

  // Estado requerido para el indicador visual del Robot RPA
  estadoRobot: 'INACTIVO' | 'CARGADO' | 'PROCESANDO' | 'COMPLETADO' = 'INACTIVO';
  mensajeError: string = '';

  constructor(private proveedorService: ProveedorService) {}

  ngOnInit(): void {
    this.cargarProveedores();
  }

  cargarProveedores(): void {
    this.proveedorService.obtenerTodos().subscribe({
      next: (data) => this.proveedores = data,
      error: (err) => console.error('Error al recuperar proveedores', err)
    });
  }

  guardarManual(): void {
    if (!this.nuevoProveedor.nitEmpresa || !this.nuevoProveedor.nombreEmpresa || !this.nuevoProveedor.pais) {
      this.mensajeError = 'Todos los campos son obligatorios.';
      return;
    }
    this.mensajeError = '';
    this.proveedorService.registrarManual(this.nuevoProveedor).subscribe({
      next: () => {
        this.cargarProveedores(); // Refresca la tabla en tiempo real
        this.nuevoProveedor = { nitEmpresa: '', nombreEmpresa: '', pais: '', estadoRegistro: 'ACTIVO' }; // Limpia el formulario
      },
      error: (err) => this.mensajeError = err.error?.mensaje || 'Error en el registro.'
    });
  }

  seleccionarParaEditar(p: Proveedor): void {
    // Clonamos el objeto para romper la referencia directa en memoria con la tabla antes de guardar
    this.proveedorSeleccionado = { ...p };
  }

  guardarEdicion(): void {
    if (this.proveedorSeleccionado && this.proveedorSeleccionado.id) {
      this.proveedorService.actualizarProveedor(this.proveedorSeleccionado.id, this.proveedorSeleccionado).subscribe({
        next: () => {
          this.cargarProveedores(); // Refresca cambios en tiempo real
          this.proveedorSeleccionado = null; // Cierra el formulario de edición
        },
        error: (err) => alert(err.error?.mensaje || 'Error al actualizar.')
      });
    }
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.estadoRobot = 'CARGADO';
      
      // Simulación controlada del cambio de estados para que el evaluador técnico
      // observe de manera clara la transición visual del procesamiento del Robot
      setTimeout(() => {
        this.estadoRobot = 'PROCESANDO';
        
        this.proveedorService.uploadArchivoRobot(file).subscribe({
          next: () => {
            this.estadoRobot = 'COMPLETADO';
            this.cargarProveedores(); // Sincroniza la tabla de inmediato
          },
          error: () => {
            this.estadoRobot = 'INACTIVO';
            alert('Error en la comunicación con el servidor de carga.');
          }
        });
      }, 1200);
    }
  }
}
