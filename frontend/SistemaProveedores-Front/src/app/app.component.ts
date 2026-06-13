import { Component } from '@angular/core';
import { ProveedoresComponent } from './components/proveedores/proveedores.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  imports: [ProveedoresComponent] // Importa el componente standalone aquí
})
export class AppComponent {
  title = 'SistemaProveedores-Front';
}