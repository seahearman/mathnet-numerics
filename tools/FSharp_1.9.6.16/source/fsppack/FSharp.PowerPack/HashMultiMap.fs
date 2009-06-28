// (c) Microsoft Corporation 2005-2009. 


#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities.Collections
//open Internal.Utilities.Collections
#else
namespace Microsoft.FSharp.Collections
#endif

open System
open System.Collections.Generic
open Microsoft.FSharp.Collections
                                 
// Each entry in the HashMultiMap dictionary has at least one entry. Under normal usage each entry has _only_
// one entry. So use two hash tables: one for the main entries and one for the overflow.
[<Sealed>]
type HashMultiMap<'Key,'Value>(hasheq: IEqualityComparer<'Key>,n:int) = 
    let firstEntries = new Dictionary<_,_>(n,hasheq);
    let rest = new Dictionary<_,_>(3,hasheq);
 
    static member Create(hasheq,n) = new HashMultiMap<'Key,'Value>(hasheq,n)

    [<OverloadID("Create_size")>]
    static member inline Create(n) : HashMultiMap<'Key,'Value> = HashMultiMap.Create(HashIdentity.Structural,n)

    static member inline Create() : HashMultiMap<'Key,'Value> = HashMultiMap.Create(11)

    [<OverloadID("Create_seq")>]
    static member Create(seq : seq<'Key * 'Value>) : HashMultiMap<'Key,'Value> = 
        let x = new HashMultiMap<'Key,'Value>(11)
        seq |> Seq.iter (fun (k,v) -> x.Add(k,v))
        x

    [<OverloadID("new_seq")>]
    new (seq : seq<'Key * 'Value>) as x = 
        new HashMultiMap<'Key,'Value>(11)
        then 
           seq |> Seq.iter (fun (k,v) -> x.Add(k,v))

    [<OverloadID("new_size")>]
    new (n:int) = new HashMultiMap<'Key,'Value>(HashIdentity.Structural,n)
    new () = new HashMultiMap<'Key,'Value>(11)


    member x.GetRest(k) = 
        let mutable res = []
        let ok = rest.TryGetValue(k,&res)
        if ok then res else []

    member x.Add(y,z) = 
        let mutable res = Unchecked.defaultof<'Value>
        let ok = firstEntries.TryGetValue(y,&res)
        if ok then 
            rest.[y] <- res :: x.GetRest(y)
        firstEntries.[y] <- z

    member x.Clear() = 
         firstEntries.Clear()
         rest.Clear()

    member x.FirstEntries = firstEntries
    member x.Rest = rest
    member x.Copy() = 
        let res = HashMultiMap<'Key,'Value>.Create(firstEntries.Comparer, firstEntries.Count) 
        for kvp in firstEntries do 
             res.FirstEntries.Add(kvp.Key,kvp.Value)
        for kvp in rest do 
             res.Rest.Add(kvp.Key,kvp.Value)
        res

    member x.Item 
        with get(y : 'Key) = 
            let mutable res = Unchecked.defaultof<'Value>
            let ok = firstEntries.TryGetValue(y,&res)
            if ok then res else raise (new System.Collections.Generic.KeyNotFoundException("The item was not found in collection"))
        and set (y:'Key) (z:'Value) = 
            x.Replace(y,z)

    member x.FindAll(y) = 
        let mutable res = Unchecked.defaultof<'Value>
        let ok = firstEntries.TryGetValue(y,&res)
        if ok then res :: x.GetRest(y) else []

    member x.Fold f acc = 
        let mutable res = acc
        for kvp in firstEntries do
            res <- f kvp.Key kvp.Value res
            match x.GetRest(kvp.Key)  with
            | [] -> ()
            | rest -> 
                for z in rest do
                    res <- f kvp.Key z res
        res

    member x.Iterate(f) =  
        for kvp in firstEntries do
            f kvp.Key kvp.Value
            match x.GetRest(kvp.Key)  with
            | [] -> ()
            | rest -> 
                for z in rest do
                    f kvp.Key z

    member x.Contains(y) = firstEntries.ContainsKey(y)

    member x.ContainsKey(y) = firstEntries.ContainsKey(y)

    member x.Remove(y) = 
        let mutable res = Unchecked.defaultof<'Value>
        let ok = firstEntries.TryGetValue(y,&res)
        // Note, if not ok then nothing to remove - nop
        if ok then 
            // We drop the FirstEntry. Here we compute the new FirstEntry and residue MoreEntries
            let mutable res = []
            let ok = rest.TryGetValue(y,&res)
            if ok then 
                match res with 
                | [h] -> 
                    firstEntries.[y] <- h; 
                    rest.Remove(y) |> ignore
                | (h::t) -> 
                    firstEntries.[y] <- h
                    rest.[y] <- t
                | _ -> 
                    // note: broken invariant
                    ()
            else
                firstEntries.Remove(y) |> ignore 

    member x.Replace(y,z) = 
        firstEntries.[y] <- z

    member x.TryFind(y) = 
        let mutable res = Unchecked.defaultof<'Value>
        let ok = firstEntries.TryGetValue(y,&res)
        if ok then Some(res) else None

    member x.Count = firstEntries.Count

    interface IEnumerable<KeyValuePair<'Key, 'Value>> with
        member s.GetEnumerator() = 
            let elems = new System.Collections.Generic.List<_>(firstEntries.Count + rest.Count)
            for kvp in firstEntries do
                elems.Add(kvp)
                for z in s.GetRest(kvp.Key) do
                   elems.Add(KeyValuePair(kvp.Key, z))
            (elems.GetEnumerator() :> IEnumerator<_>)

    interface System.Collections.IEnumerable with
        member s.GetEnumerator() = ((s :> seq<_>).GetEnumerator() :> System.Collections.IEnumerator)

    interface IDictionary<'Key, 'Value> with 
        member s.Item 
            with get x = s.[x]            
            and  set x v = s.[x] <- v
            
        member s.Keys = ([| for kvp in s -> kvp.Key |] :> ICollection<'Key>)
        member s.Values = ([| for kvp in s -> kvp.Value |] :> ICollection<'Value>)
        member s.Add(k,v) = s.[k] <- v
        member s.ContainsKey(k) = s.ContainsKey(k)
        member s.TryGetValue(k,r) = if s.ContainsKey(k) then (r <- s.[k]; true) else false
        member s.Remove(k:'Key) = 
            let res = s.ContainsKey(k) in 
            s.Remove(k); res

    interface ICollection<KeyValuePair<'Key, 'Value>> with 
        member s.Add(x) = s.[x.Key] <- x.Value
        member s.Clear() = s.Clear()            
        member s.Remove(x) = 
            let res = s.ContainsKey(x.Key) in 
            if res && s.[x.Key] = x.Value then 
                s.Remove(x.Key); 
            res
        member s.Contains(x) = s.ContainsKey(x.Key) && s.[x.Key] = x.Value
        member s.CopyTo(arr,arrIndex) = s |> Seq.iteri (fun j x -> arr.[arrIndex+j] <- x)
        member s.IsReadOnly = false
        member s.Count = s.Count

