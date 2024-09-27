using System;
using System.Collections;
using Google.Protobuf;
using Google.Protobuf.Examples.AddressBook;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;

public class Example : MonoBehaviour
{
    private static readonly Unity.Profiling.ProfilerMarker s_PMPostProto_Send_ToByteArray
        = new Unity.Profiling.ProfilerMarker("PostProto.Send.ToByteArray");
    private static readonly Unity.Profiling.ProfilerMarker s_PMPostProto_Recv_Create
        = new Unity.Profiling.ProfilerMarker("PostProto.Recv.Create");
    private static readonly Unity.Profiling.ProfilerMarker s_PMPostProto_Recv_MergeFrom
        = new Unity.Profiling.ProfilerMarker("PostProto.Recv.MergeFrom");

    const string Url = "http://127.0.0.1:3000";
    public IEnumerator PostProto<TRecv>(IMessage data, Action<TRecv> callback) where TRecv : class, IMessage
    {
        Debug.Log($"Request.PostProto:{Url} Start");

        s_PMPostProto_Send_ToByteArray.Begin();
        var sendBytes = data.ToByteArray();
        s_PMPostProto_Send_ToByteArray.End();

        var postRequest = new UnityWebRequest(Url, UnityWebRequest.kHttpVerbPOST)
        {
            downloadHandler = new DownloadHandlerBuffer(),
            uploadHandler = new UploadHandlerRaw(sendBytes),
            disposeUploadHandlerOnDispose = true,
            disposeDownloadHandlerOnDispose = true
        };

        postRequest.SetRequestHeader("Content-Type", "application/x-protobuf");
        postRequest.timeout = 10;
        postRequest.SendWebRequest();

        while (!postRequest.isDone)
        {
            yield return null;
        }

        switch (postRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError($"Request.PostProto:{Url} Failed:{postRequest.error}");
                break;
            case UnityWebRequest.Result.Success:
                {
                    Debug.Log($"Request.PostProto:{Url} Success");
                    var recvBytes = postRequest.downloadHandler.data;

                    s_PMPostProto_Recv_Create.Begin();
                    var result = Activator.CreateInstance<TRecv>();
                    s_PMPostProto_Recv_Create.End();

                    s_PMPostProto_Recv_MergeFrom.Begin();
                    result.MergeFrom(recvBytes);
                    s_PMPostProto_Recv_MergeFrom.End();

                    callback(result);
                }
                break;
        }

        postRequest.Dispose();
    }

    IEnumerator Start()
    {
        Debug.Log("Hello World!");

        var delay = new WaitForSeconds(1);

        yield return delay;

        Person john = new Person
        {
            Id = 1234,
            Name = "John Doe",
            Email = "jdoe@example.com",
            Phones = { new Person.Types.PhoneNumber { Number = "555-4321", Type = Person.Types.PhoneType.Home } }
        };

        Person wtf = new Person { };

        // var bytes = new byte[1024];
        // CodedOutputStream output = new CodedOutputStream(bytes);
        while (true)
        {
            yield return delay;

            yield return PostProto(john, (Person person) =>
            {
                // Debug.Log(person);
            });

            // Profiler.BeginSample("PackProto");
            // output.WriteRawMessage(john);
            // Profiler.EndSample();

            // var length = 1024 - output.SpaceLeft;
            // Debug.Log(BytesToString(bytes, length));

            // var tobytes = john.ToByteArray();
            // Debug.Log(BytesToString(tobytes, tobytes.Length));

            // Profiler.BeginSample("ParseProto");
            // wtf.MergeFrom(bytes, 0, length);
            // Profiler.EndSample();

            // Debug.Log(wtf);
        }
    }


    public static void PackProto(IMessage data, ref Span<byte> output)
    {
        MessageExtensions.WriteTo(data, output);
    }

    public static void ParseProto<T>(T obj, ReadOnlySpan<byte> span) where T : class, IMessage
    {
        MessageExtensions.MergeFrom(obj, span);
    }
    private string BytesToString(byte[] bytes, int length)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("<bytes length=");
        sb.Append(length);
        sb.Append(" ");
        for (int i = 0; i < length; i++)
        {
            sb.Append(bytes[i].ToString("X2"));
            sb.Append(" ");
        }
        sb.Append(">");
        return sb.ToString();
    }

    public void TestProto(Person john, Person wtf, Span<byte> span)
    {
        Profiler.BeginSample("PackProto");
        // PackProto(john, ref span);
        MessageExtensions.WriteTo(john, span);
        Profiler.EndSample();
        Debug.Log(span.Length);

        Profiler.BeginSample("ParseProto");
        ParseProto(wtf, span);
        Profiler.EndSample();
        Debug.Log(wtf);
    }
}