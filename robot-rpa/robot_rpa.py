import os
import csv
import time
import logging
import requests
import urllib3

# Desactivar advertencias de SSL para desarrollo local
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# Configuración de Logs profesional para RPA
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
    handlers=[
        logging.FileHandler("rpa_execution.log", encoding="utf-8"),
        logging.StreamHandler()
    ]
)

class ProveedoresRobotRPA:
    def __init__(self, api_url, archivo_csv):
        self.api_url = api_url.rstrip('/')
        self.archivo_csv = archivo_csv
        self.registros_procesados = 0
        self.registros_exitosos = 0
        self.registros_fallidos = 0
        self.session = requests.Session()

    def conectar_api(self):
        """Verifica la existencia del servicio."""
        logging.info("Verificando conectividad con el backend de .NET...")
        try:
            response = self.session.get(f"{self.api_url}/proveedores", timeout=5, verify=False)
            if response.status_code == 200:
                logging.info("Conexión base establecida con éxito (Estatus 200).")
                return True
            else:
                logging.error(f"El backend respondió con código: {response.status_code}")
                return False
        except requests.exceptions.RequestException as e:
            logging.critical(f"No se pudo conectar a la API: {e}")
            return False

    def enviar_registro(self, payload, headers):
        """Prueba los diferentes esquemas y rutas hasta que uno responda exitosamente (Evita el 405)."""
        # Lista exhaustiva de endpoints de inserción en .NET Web API
        rutas_a_probar = [
            f"{self.api_url}/proveedores/manual",
            f"{self.api_url}/proveedores/cargar",
            f"{self.api_url}/proveedores",
            f"{self.api_url}/proveedores/crear"
        ]

        for url in rutas_a_probar:
            try:
                # Intento 1: Objeto plano directo en la raíz
                response = self.session.post(url, json=payload, headers=headers, timeout=3, verify=False)
                if response.status_code in [200, 201]:
                    return True, response.status_code, response.text
                
                # Intento 2: Si da 400/415, probar envolviendo en "payload" por si el DTO lo exige
                if response.status_code in [400, 415]:
                    response_wrapped = self.session.post(url, json={"payload": payload}, headers=headers, timeout=3, verify=False)
                    if response_wrapped.status_code in [200, 201]:
                        return True, response_wrapped.status_code, response_wrapped.text
                        
            except requests.exceptions.RequestException:
                continue
                
        return False, 405, "Method Not Allowed en todos los endpoints probados."

    def ejecutar_automatizacion(self):
        """Lee el archivo plano e ingesta los datos."""
        if not os.path.exists(self.archivo_csv):
            logging.error(f"El archivo '{self.archivo_csv}' no existe.")
            return

        if not self.conectar_api():
            return

        logging.info(f"Procesando archivo: {self.archivo_csv}")
        
        with open(self.archivo_csv, mode='r', encoding='utf-8') as file:
            sample = file.read(2048)
            dialect = None
            if sample:
                try:
                    dialect = csv.Sniffer().sniff(sample)
                except csv.Error:
                    dialect = None
            file.seek(0)
            
            reader = csv.DictReader(file, dialect=dialect if dialect else 'excel')
            
            headers_api = {
                "Content-Type": "application/json",
                "Accept": "application/json"
            }
            
            for fila in reader:
                self.registros_processed_actual = self.registros_procesados + 1
                self.registros_procesados += 1
                
                # Normalización de columnas
                fila_limpia = {k.strip().lower(): v for k, v in fila.items() if k is not None}

                nit = (fila_limpia.get("nit_empresa") or fila_limpia.get("nitempresa") or fila_limpia.get("nit") or "").strip()
                nombre = (fila_limpia.get("nombre_empresa") or fila_limpia.get("nombreempresa") or fila_limpia.get("nombre") or "").strip()
                pais = (fila_limpia.get("pais") or fila_limpia.get("país") or "").strip()
                estado = (fila_limpia.get("estado_registro") or fila_limpia.get("estadoregistro") or fila_limpia.get("estado") or "ACTIVO").strip()

                if not nit or not nombre:
                    self.registros_fallidos += 1
                    continue

                payload = {
                    "nitEmpresa": nit,
                    "nombreEmpresa": nombre,
                    "pais": pais,
                    "tipoCarga": "AUTOMATICO",
                    "estadoRegistro": estado
                }

                # Ejecutar la estrategia de evasión del error 405/400
                exito, code, text = self.enviar_registro(payload, headers_api)

                if exito:
                    logging.info(f"✔ [Fila {self.registros_procesados}] Insertado correctamente: {payload['nombreEmpresa']}")
                    self.registros_exitosos += 1
                else:
                    logging.error(f"❌ [Fila {self.registros_procesados}] Error en Servidor ({code}): {text}")
                    self.registros_fallidos += 1
                
                time.sleep(0.1)

        self.generar_reporte_cierre()

    def generar_reporte_cierre(self):
        logging.info("==================================================")
        logging.info("        RESUMEN DE EJECUCIÓN DEL ROBOT RPA        ")
        logging.info("==================================================")
        logging.info(f" Total registros leídos:   {self.registros_procesados}")
        logging.info(f" Procesados con éxito:     {self.registros_exitosos}")
        logging.info(f" Errores / Descartados:    {self.registros_fallidos}")
        logging.info("==================================================")

if __name__ == "__main__":
    API_URL_LOCAL = "https://localhost:53528/api" 
    ARCHIVO_INSUMO = "proveedores_rpa_prueba.csv"

    robot = ProveedoresRobotRPA(api_url=API_URL_LOCAL, archivo_csv=ARCHIVO_INSUMO)
    robot.ejecutar_automatizacion()