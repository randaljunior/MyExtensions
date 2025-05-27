using System.Reflection;

namespace MyExtensions;

public static partial class ObjectExtensions
{
    /// <summary>
    /// The Clone Method that will be recursively used for the deep clone.
    /// </summary>
    private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>
    /// Returns TRUE if the type is a primitive one, FALSE otherwise.
    /// </summary>
    private static bool IsPrimitive(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type == typeof(string) || (type.IsValueType && type.IsPrimitive);
    }

    /// <summary>
    /// Returns a Deep Clone / Deep Copy of an object using a recursive call to the CloneMethod specified above.
    /// </summary>
    private static object? DeepClone(this object obj)
    {
        return DeepClone_Internal(obj, new Dictionary<object, object>(new ReferenceEqualityComparer()));
    }

    /// <summary>
    /// Returns a Deep Clone / Deep Copy of an object of type T using a recursive call to the CloneMethod specified above.
    /// </summary>
    public static T? DeepClone<T>(this T obj)
    {
        if(obj is null)
            return default;

        return (T)DeepClone((object)obj);
    }

    private static object? DeepClone_Internal(object obj, IDictionary<object, object> visited)
    {
        if (obj is null)
            return null;

        Type typeToReflect = obj.GetType();

        // Se o objeto for "primitivo" (ou imutável como string), retornamos diretamente.
        if (IsPrimitive(typeToReflect))
            return obj;

        // Se o objeto já foi clonado (para referências cíclicas), retornamos o clone.
        if (visited.ContainsKey(obj))
            return visited[obj];

        // Evita a clonagem de delegates.
        if (typeof(Delegate).IsAssignableFrom(typeToReflect))
            return null;

        // Cria um clone superficial via reflexão (CloneMethod é um MethodInfo apontando para o método de clonagem)
        object? cloneObject = CloneMethod.Invoke(obj, null);

        // Registra o clone imediatamente para evitar ciclos.
        visited.Add(obj, cloneObject);

        // Se for um array, processa os elementos individualmente
        if (typeToReflect.IsArray)
        {
            // Para arrays, é preferível iterar sobre os elementos do array original,
            // pois o clone é apenas uma cópia superficial (seu conteúdo ainda é o mesmo)
            Array originalArray = (Array)obj;
            Array clonedArray = (Array)cloneObject;
            var elementType = typeToReflect.GetElementType();

            // Se os elementos não forem primitivos (ou imutáveis), fazemos o deep clone
            if (!IsPrimitive(elementType))
            {
                // 'ForEach' é um método auxiliar que itera sobre os índices do array.
                clonedArray.ForEach((array, indices) =>
                {
                    // Para cada posição, obtém o elemento do array original,
                    // faz o deep clone e atualiza o array clonado.
                    array.SetValue(DeepClone_Internal(originalArray.GetValue(indices), visited), indices);
                });
            }
        }

        // Copia os campos do objeto original para o clone (para campos públicos, privados e herdados)
        CopyFields(obj, visited, cloneObject, typeToReflect);
        RecursiveCopyBaseTypePrivateFields(obj, visited, cloneObject, typeToReflect);

        return cloneObject;
    }

    private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
    {
        // Obtem o tipo base uma vez para evitar múltiplas chamadas
        var baseType = typeToReflect.BaseType;
        if (baseType is not null)
        {
            // Primeiro, recorre para copiar os campos privados da classe base mais alta
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, baseType);

            // Em seguida, copia os campos privados deste nível da classe base
            CopyFields(originalObject, visited, cloneObject, baseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
        }
    }

    private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
    {
        var fields = typeToReflect.GetFields(bindingFlags);
        foreach (FieldInfo fieldInfo in fields)
        {
            // Caso opte por avaliar primitividade antes do filtro (ajuste se necessário):
            if (IsPrimitive(fieldInfo.FieldType) || (filter != null && !filter(fieldInfo)))
                continue;

            var originalFieldValue = fieldInfo.GetValue(originalObject);
            // Se necessário, trate o caso de null explicitamente
            if (originalFieldValue == null)
            {
                fieldInfo.SetValue(cloneObject, null);
                continue;
            }
            var clonedFieldValue = DeepClone_Internal(originalFieldValue, visited);
            fieldInfo.SetValue(cloneObject, clonedFieldValue);
        }
    }

    private static void ForEach(this Array array, Action<Array, int[]> action)
    {
        // Valida os argumentos, evitando NRE
        ArgumentNullException.ThrowIfNull(array, nameof(array));
        ArgumentNullException.ThrowIfNull(action, nameof(action));

        // Se o array estiver vazio, não há nada para percorrer
        if (array.LongLength == 0)
            return;

        // Cria o iterador para percorrer todas as posições
        var walker = new ArrayTraverse(array);

        // Itera "do-while", executando a ação para cada posição
        // Note que walker.Position é um array que é reutilizado em cada iteração.
        // Se a ação armazenar essa referência para uso posterior, considere clonar a posição.
        do
        {
            action(array, walker.Position);
        }
        while (walker.Step());
    }
}

internal class ReferenceEqualityComparer : EqualityComparer<object>
{
    public override bool Equals(object? x, object? y)
    {
        return ReferenceEquals(x, y);
    }

    public override int GetHashCode(object obj)
    {
        if (obj == null) return 0;
        return obj.GetHashCode();
    }
}

internal class ArrayTraverse
{
    public int[] Position;
    private readonly int[] maxLengths;

    public ArrayTraverse(Array array)
    {
        int rank = array.Rank;
        maxLengths = new int[rank];
        for (int i = 0; i < rank; ++i)
        {
            maxLengths[i] = array.GetLength(i) - 1;
        }
        Position = new int[rank];
    }

    public bool Step()
    {
        int rank = Position.Length;
        for (int i = 0; i < rank; ++i)
        {
            if (Position[i] < maxLengths[i])
            {
                Position[i]++;
                if (i > 0)
                {
                    Array.Clear(Position, 0, i);
                }
                return true;
            }
        }
        return false;
    }
}
