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
  isDragging: boolean = false; // Controla el estado visual de la zona de arrastre

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

  onFileSelected(event: Event): void {
    const element = event.target as HTMLInputElement;
    const file = element.files ? element.files[0] : null;
    if (file) {
      this.procesarArchivoRPA(file);
      element.value = ''; // Limpiar el input para permitir subir el mismo archivo tras un error
    }
  }

  // Lógica para Zona de Arrastre (Drag & Drop)
  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave(): void {
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
    const file = event.dataTransfer?.files[0];
    if (file) {
      this.procesarArchivoRPA(file);
    }
  }

  private procesarArchivoRPA(file: File): void {
    this.mensajeError = '';
    this.estadoRobot = 'CARGADO';
    
    // Transición visual para que el usuario perciba el trabajo del "Robot"
    setTimeout(() => {
      this.estadoRobot = 'PROCESANDO';
      
      this.proveedorService.uploadArchivoRobot(file).subscribe({
        next: () => {
          this.estadoRobot = 'COMPLETADO';
          this.cargarProveedores(); // Refrescar la tabla para ver los nuevos datos
        },
        error: (err) => {
          this.estadoRobot = 'INACTIVO';
          this.mensajeError = err.error?.mensaje || 'Error al procesar el archivo CSV.';
          console.error('Error RPA:', err);
        }
      });
    }, 1500);
  }
}
