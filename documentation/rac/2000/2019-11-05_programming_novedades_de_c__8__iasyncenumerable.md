```yaml
---
title: "Novedades de C# 8: IAsyncEnumerable"
source: https://www.fixedbuffer.com/novedades-de-c-8-iasyncenumerable/
date_published: 2019-11-05T09:05:48.000Z
date_captured: 2025-08-27T10:50:55.970Z
domain: www.fixedbuffer.com
author: JorTurFer
category: programming
technologies: [.NET Core 3.0, C# 8.0, .NET Standard 2.1, IAsyncEnumerable, IEnumerable, Task, "Span<T>", Entity Framework Core 3.0, API Rest]
programming_languages: [C#]
tags: [csharp, async, performance, dotnet, net-core, iasyncenumerable, asynchronous-streams, yield, iteration, new-features]
key_concepts: [asynchronous-programming, asynchronous-streams, yield-return, performance-optimization, lazy-evaluation, iterators, task-based-asynchronous-pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces `IAsyncEnumerable<T>`, a significant new feature in C# 8.0, released alongside .NET Core 3.0. It addresses the challenge of asynchronously iterating collections as elements are generated, rather than waiting for the entire collection to be fully constructed. Through clear C# code examples, the author demonstrates how `IAsyncEnumerable<T>`, combined with `await foreach` and `yield return`, offers substantial performance improvements and enhanced code readability compared to traditional `Task<IEnumerable<T>>` approaches. This feature is particularly beneficial for scenarios involving data retrieval from external sources like databases or APIs, enabling more efficient, stream-based data processing. The article emphasizes the performance gains and simplified asynchronous logic provided by this new capability.
---
```

# Novedades de C# 8: IAsyncEnumerable

# Novedades de C# 8: IAsyncEnumerable

11 febrero, 20205 noviembre, 2019 por [JorTurFer](https://www.fixedbuffer.com/author/kabestrus/ "Ver todas las entradas de JorTurFer")

Tiempo de lectura: 4 minutos

![C# 8.0 logo, a purple hexagon with a white C# symbol, and "8.0" next to it on a blue background.](https://www.fixedbuffer.com/wp-content/uploads/2019/11/c8.jpg)

¿Y qué es eso de IAsyncEnumerable? Hace algo más de un mes que se libreró .Net Core 3.0 y ya hemos hablado sobre novedades que nos trae (que no son pocas…). Pero sin duda, una de las principales novedades que se liberaron junto a .Net Core 3, fue C# 8.0.

C# 8 nos ofrece una interesante cantidad de novedades de las que vamos a ir hablando durante las próximas semanas:

*   Tipos nullables
*   Rangos e índices
*   Declaraciones using
*   Interfaces con implementaciones por defecto
*   Reconocimiento de patrones
*   Asignaciones de uso combinado

Pero para mí el más importante en cuanto al impacto que puede tener en el rendimiento de nuestra aplicación, sin duda alguna, es "[IAsyncEnumerable](https://docs.microsoft.com/es-es/dotnet/api/system.collections.generic.iasyncenumerable-1?view=dotnet-plat-ext-3.0)".

> Hay otros cambios que afecta mucho más al rendimiento, como Span<T> por ejemplo, pero es más difícil que lo usemos en nuestro día a día que IAsyncEnumerable

Hasta ahora, no teníamos manera de iterar un objeto de manera asíncrona medida que se va generando. ¿Qué quiere decir esto? ¡Pues fácil! Imagina que tenemos un método que nos devuelve una colección de 100 objetos y que, por ejemplo, tarda en generar cada objeto 1 segundo. Una vez que tenemos la colección lista, la iteramos con un foreach que tarda otro segundo en realizar un ciclo. Algo así:

```csharp
async Task<IEnumerable<int>> GetCountIEnumerableAsync()
{
    var list = new List<int>();
    for (int i = 0; i < 100; i++)
    {
        await Task.Delay(1000); 
        list.Add(i);
    }

    return list;
}

//....

foreach (var valor in await GetCountIEnumerableAsync())
{
    //Acción que tarda un segundo en ejecutarse
}
```

Con el código anterior, asumiendo 1 segundo para generar cada objeto de la colección y un segundo para procesarlo, vamos a tardar 200 segundos en ejecutar el foreach completo. Hasta ahora, no nos quedaba otra que gestionar nosotros la situación para obtener los datos por un lado y procesarlos por otro para poder darle salida nada más obtenerlos, obligándonos a implementar una lógica extra que hay que mantener y sincronizar.

## IAsyncEnumerable<T> al rescate

Como decíamos antes, con C# 8 se han introducido los streams asíncronos a través de la interfaz IAsyncEnumerable<T>. Esto lo que nos va a permitir es no tener que esperar a que la colección se obtenga completa antes de empezar a iterarla. Volviendo al ejemplo anterior, si en obtener cada dato tardamos un segundo, pero mientras obtenemos un nuevo dato procesamos el anterior, **en vez de 200 segundos, solo vamos a tardar 101** (aproximadamente). Vamos a verlo con un poco de código:

```csharp
async IAsyncEnumerable<int> GetCountIAsyncEnumerableAsync()
{
    for (int i = 0; i < 100; i++)
    {
        await Task.Delay(1000); 
        yield return i;
    }
}

//....

await foreach (var valor in GetCountIAsyncEnumerableAsync())
{
    //Acción que tarda un segundo en ejecutarse
}
```

Este código va a ir despachando cada uno de los objetos a medida que se van generando nuestro método "GetCountIAsyncEnumerableAsync". Como puedes comprobar esto es infinitamente más fácil y más legible. Imagina si tuviésemos que gestionar dos hilos independientes, uno para adquirir datos y otro para procesarlos…

Si los datos provienen de algún servicio externo como una base de datos o un API Rest (o cualquier otro origen en general). Como resultado, vamos a obtener una mejora bastante importante gracias a esto. Ya sea por aumentar el rendimiento, o por facilitarnos el código. ¿Aun no tienes claro cómo funciona?

## Vamos a ver un ejemplo claro

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAsyncEnumerable
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Ejecutando foreach con Task<IEnumerable<int>>");
            foreach (var valor in await GetCountIEnumerableAsync())
            {
                Console.WriteLine($"Procesado el valor {valor}");
            }

            Console.WriteLine("Ejecutando foreach con IAsyncEnumerable<int>");
            await foreach (var valor in GetCountIAsyncEnumerableAsync())
            {
                Console.WriteLine($"Procesado el valor {valor}");
            }

            Console.Read();
        }

        private static async IAsyncEnumerable<int> GetCountIAsyncEnumerableAsync()
        {
            for (var i = 0; i < 3; i++)
            {
                await Task.Delay(1000);
                Console.WriteLine($"Añadido valor {i}");
                yield return i;
            }
        }

        private static async Task<IEnumerable<int>> GetCountIEnumerableAsync()
        {
            var list = new List<int>();
            for (var i = 0; i < 3; i++)
            {
                await Task.Delay(1000);
                Console.WriteLine($"Añadido valor {i}");
                list.Add(i);
            }
            return list;
        }
    }
}
```

En el código anterior, simplemente hemos implementado ambos casos, uno con IAsyncEnumerable<int> y otro con Task<IEnumerable<int>>. Si ejecutamos el código la salida que obtenemos es algo como esto:

![Console output demonstrating the execution flow difference between Task<IEnumerable<int>> (first, sequential adding then processing) and IAsyncEnumerable<int> (second, interleaved adding and processing).](https://www.fixedbuffer.com/wp-content/uploads/2019/11/image.png)

Con la imagen se ve más claro cuál es el cambio de funcionamiento que nos porta esta nueva característica, pero… ¡Si tienes alguna duda, puedes dejar un comentario!

Durante las próximas semanas vamos a seguir hablando de las interesantes novedades que nos aporta C# 8. No te lo pierdas, realmente es muy interesante. Si además eres una persona preocupada por el rendimiento, te dejo un enlace sobre como escribir [código de alto rendimiento en .Net Core](https://www.fixedbuffer.com/escribiendo-codigo-de-alto-rendimiento-en-net/).