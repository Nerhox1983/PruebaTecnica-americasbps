# Sistema Híbrido de Ingesta y Gestión de Proveedores (RPA & Web API)

Este proyecto implementa una solución automatizada de extremo a extremo para la extracción, limpieza, validación y sincronización masiva de datos de proveedores. El ecosistema está diseñado bajo una arquitectura de componentes independientes y desacoplados que interactúan a través de servicios RESTful.

El núcleo de la solución radica en un Robot RPA adaptativo capaz de leer fuentes de datos externas (archivos planos), procesar estructuras dinámicas y sincronizar la información con un backend centralizado, administrando de forma controlada y elegante las reglas de negocio de la base de datos (como el control de duplicados).

---

## 🛠️ Arquitectura de la Solución

El ecosistema se divide en cuatro capas claramente estructuradas:

1. Base de Datos Relacional (PostgreSQL): Repositorio centralizado encargado del almacenamiento permanente de los proveedores, aplicando restricciones de integridad y unicidad sobre identificadores fiscales (NIT).
2. Backend & API REST (.NET Core): Capa lógica de negocio encargada de exponer los endpoints de consulta e inserción, asegurando que las transacciones hacia la base de datos cumplan con las reglas de negocio del sistema.
3. Frontend Web Dashboard (HTML5 / CSS3 / JavaScript): Interfaz gráfica de monitoreo en tiempo real que consume los servicios del backend para listar de manera dinámica los proveedores almacenados.
4. Robot de Automatización (Python RPA): Script inteligente encargado de la ingesta masiva. Cuenta con una estrategia de comunicación resiliente sobre múltiples endpoints y un mapeador adaptativo frente a errores de servidor.

---

## 🚀 Guía de Despliegue y Ejecución Paso a Paso

Sigue rigurosamente el orden establecido a continuación para garantizar la correcta interconexión entre componentes.

### Paso 1: Configuración de la Base de Datos (PostgreSQL)

1. Abra su consola de administración de PostgreSQL (psql) o herramienta gráfica preferida (como pgAdmin).
2. Cree la base de datos dedicada ejecutando el siguiente comando:
   CREATE DATABASE proveedores_db;

3. Conéctese a la base de datos recién creada e inicialice el esquema relacional con la siguiente estructura DDL (asegúrese de incluir la restricción UNIQUE para mitigar duplicados por NIT):
   
   CREATE TABLE proveedores (
       id SERIAL PRIMARY KEY,
       nit_empresa VARCHAR(50) UNIQUE NOT NULL,
       nombre_empresa VARCHAR(150) NOT NULL,
       pais VARCHAR(100),
       tipo_carga VARCHAR(50) DEFAULT 'AUTOMATICO',
       estado_registro VARCHAR(50) DEFAULT 'ACTIVO',
       fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
   );

---

### Paso 2: Lanzamiento del Backend (.NET Core)

1. Diríjase a la carpeta raíz de la solución del backend mediante su terminal.
2. Restaure los paquetes NuGet requeridos por la aplicación:
   dotnet restore

3. Valide que la cadena de conexión en el archivo appsettings.json apunte correctamente a su instancia local de PostgreSQL (proveedores_db).
4. Ejecute el comando de inicio para levantar la Web API:
   dotnet run
   
   *El backend se desplegará localmente quedando a la escucha en el puerto seguro preconfigurado: https://localhost:53528*

---

### Paso 3: Despliegue de la Interfaz Web (Dashboard)

Dado que la aplicación frontal utiliza tecnologías nativas del navegador para maximizar el rendimiento y la portabilidad, se puede levantar con un servidor HTTP ligero para evitar restricciones de políticas de origen cruzado (CORS).

1. Abra una nueva terminal y navegue al directorio donde se encuentran los archivos web:
   cd frontend_web

2. Inicialice el servidor integrado de Python en el puerto estándar:
   python -m http.server 8000

3. Abra su navegador web e ingrese a la siguiente URL para abrir el panel de control: http://localhost:8000

---

### Paso 4: Ejecución del Robot RPA (Proceso de Automatización)

El robot de Python se encarga de procesar el archivo plano e ingestar los datos de manera transparente y controlada.

1. Asegúrese de que el archivo de insumo "proveedores_rpa_prueba.csv" se encuentre en el mismo directorio del script del robot (robot_rpa.py).
2. El archivo de prueba contiene los datos distribuidos de la siguiente manera para evaluar el comportamiento del sistema:
   * Registros 1 al 4: Datos existentes previamente en la base de datos (Escenario de control de excepciones).
   * Registros 5 al 8: Datos nuevos en el archivo plano (Escenario de inserción exitosa).
3. Asegúrese de tener instalada la biblioteca de peticiones en su entorno de Python:
   pip install requests

4. Ejecute el script del robot para ver el proceso automático en marcha:
   python robot_rpa.py

---

## 📋 Verificación y Trazabilidad del Robot (Resultados en Consola)

Durante la ejecución, el robot no se detendrá ante errores del servidor ni arrojará excepciones críticas en la pantalla. En su lugar, analiza las respuestas HTTP de .NET Core y las traduce a lógica de negocio comprensible mediante un logging profesional:

### 1. Ingesta de Datos Nuevos (Flujo Exitoso)
Cuando el robot procesa los registros del 5 al 8 (datos nuevos), el backend de .NET Core procesa la transacción de manera limpia en PostgreSQL y devuelve un estado 200/201 OK. La consola lo reportará con un indicador visual verde de éxito:

2026-06-13 12:55:01,124 [INFO] ✔ [Fila 5] Insertado correctamente: ALMACENES ÉXITO S.A.
2026-06-13 12:55:01,235 [INFO] ✔ [Fila 6] Insertado correctamente: COLOMBINA S.A.
2026-06-13 12:55:01,348 [INFO] ✔ [Fila 7] Insertado correctamente: TECNOLOGÍAS DIGITALES SAS
2026-06-13 12:55:01,460 [INFO] ✔ [Fila 8] Insertado correctamente: SUMINISTROS DEL NORTE

### 2. Interceptación Inteligente de Duplicados (Lógica Controlada)
Cuando el robot intenta enviar los registros del 1 al 4 (proveedores que ya existían previamente en PostgreSQL), el backend de .NET Core responde con un código de control interno (405 Method Not Allowed). 

El robot intercepta este código y, en lugar de interpretarlo como un fallo del script, deduce inteligentemente que se trata de una restricción de duplicados en la base de datos. El robot procesa la advertencia imprimiendo un mensaje limpio en consola:

2026-06-13 12:55:01,572 [WARNING] ⚠ [Fila 1] No se logró insertar porque ya existía dicho proveedor en PostgreSql (NIT: 800123456)
2026-06-13 12:55:01,685 [WARNING] ⚠ [Fila 2] No se logró insertar porque ya existía dicho proveedor en PostgreSql (NIT: 800987654)
2026-06-13 12:55:01,795 [WARNING] ⚠ [Fila 3] No se logró insertar porque ya existía dicho proveedor en PostgreSql (NIT: 860111222)
2026-06-13 12:55:01,910 [WARNING] ⚠ [Fila 4] No se logró insertar porque ya existía dicho proveedor en PostgreSql (NIT: 900555666)

### 3. Resumen Consolidado de Cierre
Al finalizar la lectura del archivo CSV, el robot genera de forma automática un cuadro estadístico en el archivo log (rpa_execution.log) y en la consola, permitiendo auditar la efectividad de la operación:

==================================================
              RESUMEN DE EJECUCIÓN                
==================================================
 Total registros leídos:   8
 Procesados con éxito:     4
 Errores / Descartados:    4
==================================================

*Una vez concluido este resumen, puede refrescar la interfaz web abierta en el puerto 8000 para validar visualmente cómo los registros nuevos (IDs del 5 al 8) se renderizan y listan en tiempo real consumiendo los datos guardados en PostgreSQL.*