using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.Franchise
{
    public static class IncidentCodes
    {
        public static Dictionary<string, string> CodeDictionary()
        {
            return new Dictionary<string, string> {
                { "R01", @"Producto no entregado al cliente y no reportado
como faltante al inicio de la ruta. Esto considera
productos reportados por el repartidor, el cliente,
atención a clientes, operaciones, mercadotecnia,
tecnología, administración o alguna otra área
involucrada. " },
                { "R02", @"Productos dañados por mal manejo en traslado.
Esto considera productos reportados por el
repartidor, el cliente, atención a clientes,
operaciones, mercadotecnia, tecnología,
administración o alguna otra área involucrada." },
                { "R03", @"Segunda visita a cliente durante el mismo día por
un mismo pedido debido a productos no
entregados en primera visita." },
                { "R04", @"Pedidos entregados fuera de horario (16 a 30
minutos tarde)." },
                { "R05", @"Pedidos entregados fuera de horario (más de 30
minutos tarde)." },
                { "R06", @"Pedidos no entregados." },
                { "R07", @"Pedido entregado en horario adelantado sin
autorización de atención a clientes." },
                { "R08", @"Reporte falso a atencion a clientes. Esto considera
cualquier información falsa que alguno de los
operarios de la ruta comunique a alguna de las
otras áreas; e.g., horario de pedido entregado
falso, ubicación de camioneta en tiempo real falsa,
reporte falso de incidencias, etc." },
                { "R09", @"Cualquier agresión físical o verbal, falta de respeto
o imprudencia realizada por el frnquiciatario, sus
empleados o terceros relacionados que afecte
directamente a cualquier cliente, proveedor,
colaborador o tercero relacionado o no relacionado." },
                { "R10", @"Cualquier violación a los reglamentos de tránsito o
cualesquiera otras leyes, reglamentos o normas
públicas o privadas (incluyendo reglamentos
internos de unidades de propiedad privada como
edificios corporativos, unidades habitacionales,
etc.) realizada por una unidad de transporte o
agluno de los fraquiciatarios, sus empleados o
terceros relacionados mientras se porta la marca
de CEL ya sea con unidad de transporte, uniforme
o cualquier otro medio que identifique a la persona
o unidad como representate de CEL. Esto incluye
incidencias realizadas dentro y fuera de los
horarios de operaciones de CEL." },
                { "R11", @"Rescate de ruta. Si una de las rutas lleva un retraso
significativo que, a consideración de los ejecutivos
de CEL, corra riesgo de no entregar pedidos, se
asignará otra ruta para apoyar con las entregas.
(penalización por día)." },
                { "P01", @"Repartidor suplente proporcionado por CEL (por
jornada)." },
                { "P02", @"Día no trabajado; i.e., no se presentó la unidad de
transporte para atender su ruta (penalización por
cada unidad de trasporte no presentada)." },
                { "P03", @"Operario en horario de trabajo que no cumpla con
el código de vestimenta, seguridad, higiene y
salubridad requerido por CEL; e.g., uniforme,
manos limpias, uñas cortas, uso de cubrebocas,
etc." },
                { "D01", @"Contacto directo de cualquier miembro de la
franquicia (empleado, franquiciatario o tercero
relacionado) con algún cliente o relacionado
(familiares, cohabitantes, trabajadores domésticos,
etc.) sin previa autorización por escrito de CEL.
Esto considera cualquier tipo de comunciación por
cualquier medio; e.g., redes sociales, mensajería
instantánea, llamada telefónica, correspondencia,
etc." },
                { "Z00", @"Penalización abierta. " },
            };
        }
    }
}