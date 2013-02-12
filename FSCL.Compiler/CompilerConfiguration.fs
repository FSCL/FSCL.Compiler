namespace FSCL.Compiler.Configuration

open System
open System.IO
open System.Reflection
open FSCL.Compiler
open System.Collections.Generic
open System.Xml
open System.Xml.Linq
open Microsoft.FSharp.Reflection

type CompilerSource =
| FileSource of string

type OverrideMode =
| Replace
| Merge
| Remove

type CompilerStepConfiguration(i: string) =
    member val ID = i with get
    member val OverrideMode = Replace with get, set
    member val Dependencies = new List<string>() with get
    member val Before = new List<string>() with get
    static member internal ToXml(s:CompilerStepConfiguration) =
        let el = new XElement(XName.Get(s.GetType().Name),
                    new XAttribute(XName.Get("ID"), s.ID),
                    new XAttribute(XName.Get("OverrideMode"), s.OverrideMode),
                    new XElement(XName.Get("Dependencies"),
                        Array.ofSeq(seq {
                            for item in s.Dependencies do
                                yield XElement(XName.Get("Item"), XAttribute(XName.Get("ID"), item))
                        })),
                    new XElement(XName.Get("Before"),
                        Array.ofSeq(seq {
                            for item in s.Before do
                                yield XElement(XName.Get("Item"), XAttribute(XName.Get("ID"), item))
                        })))
        el
    static member internal FromXml(el:XElement) =
        let conf = new CompilerStepConfiguration(el.Attribute(XName.Get("ID")).Value)
        if(el.Attribute(XName.Get("OverrideMode")) <> null) then
            conf.OverrideMode <- (ConfigurationUtil.ParseUnion<OverrideMode>(el.Attribute(XName.Get("OverrideMode")).Value)).Value
        for d in el.Elements(XName.Get("Dependencies")) do
            for item in d.Elements(XName.Get("Item")) do
                conf.Dependencies.Add(item.Attribute(XName.Get("ID")).Value)
        for d in el.Elements(XName.Get("Before")) do
            for item in d.Elements(XName.Get("Item")) do
                conf.Before.Add(item.Attribute(XName.Get("ID")).Value)
        conf

type CompilerStepProcessorConfiguration(i: string) =
    member val ID = i with get
    member val OverrideMode = Replace with get, set
    member val Dependencies = new List<string>() with get
    member val Before = new List<string>() with get
    static member internal ToXml(p: CompilerStepProcessorConfiguration) =
        let el = new XElement(XName.Get(p.GetType().Name),
                    new XAttribute(XName.Get("ID"), p.ID),
                    new XAttribute(XName.Get("OverrideMode"), p.OverrideMode),
                    new XElement(XName.Get("Dependencies"),
                        Array.ofSeq(seq {
                            for item in p.Dependencies do
                                yield XElement(XName.Get("Item"), XAttribute(XName.Get("ID"), item))
                        })),
                    new XElement(XName.Get("Before"),
                        Array.ofSeq(seq {
                            for item in p.Before do
                                yield XElement(XName.Get("Item"), XAttribute(XName.Get("ID"), item))
                        })))
        el
    static member internal FromXml(el:XElement) =
        let conf = new CompilerStepProcessorConfiguration(el.Attribute(XName.Get("ID")).Value)
        if(el.Attribute(XName.Get("OverrideMode")) <> null) then
            conf.OverrideMode <- (ConfigurationUtil.ParseUnion<OverrideMode>(el.Attribute(XName.Get("OverrideMode")).Value)).Value
        for d in el.Elements(XName.Get("Dependencies")) do
            for item in d.Elements(XName.Get("Item")) do
                conf.Dependencies.Add(item.Attribute(XName.Get("ID")).Value)
        for d in el.Elements(XName.Get("Before")) do
            for item in d.Elements(XName.Get("Item")) do
                conf.Before.Add(item.Attribute(XName.Get("ID")).Value)
        conf
    
type CompilerTypeHandlerConfiguration(i: string) =
    member val ID = i with get
    member val OverrideMode = Replace with get, set
    member val Before = new List<string>() with get
    static member internal ToXml(t: CompilerTypeHandlerConfiguration) =
        let el = new XElement(XName.Get(t.GetType().Name),
                    new XAttribute(XName.Get("ID"), t.ID),
                    new XAttribute(XName.Get("OverrideMode"), t.OverrideMode),
                    new XElement(XName.Get("Before"),
                        Array.ofSeq(seq {
                            for item in t.Before do
                                yield XElement(XName.Get("Item"), XAttribute(XName.Get("ID"), item))
                        })))
        el
    static member internal FromXml(el:XElement) =
        let conf = new CompilerTypeHandlerConfiguration(el.Attribute(XName.Get("ID")).Value)
        if(el.Attribute(XName.Get("OverrideMode")) <> null) then
            conf.OverrideMode <- (ConfigurationUtil.ParseUnion<OverrideMode>(el.Attribute(XName.Get("OverrideMode")).Value)).Value
        for d in el.Elements(XName.Get("Before")) do
            for item in d.Elements(XName.Get("Item")) do
                conf.Before.Add(item.Attribute(XName.Get("ID")).Value)
        conf

type CompilerConfiguration() =
    member val LoadDefaultSteps = true with get, set
    member val Sources = new List<CompilerSource>() with get
    member val OverrideSteps = new List<CompilerStepConfiguration>() with get
    member val OverrideProcessors = new List<CompilerStepProcessorConfiguration>() with get
    member val OverrideTypeHandlers = new List<CompilerTypeHandlerConfiguration>() with get

    static member internal ToXml(c: CompilerConfiguration) =
        let el = new XElement(XName.Get(c.GetType().Name),
                    new XAttribute(XName.Get("LoadDefaultSteps"), c.LoadDefaultSteps),
                    new XElement(XName.Get("Sources"),
                        Array.ofSeq(seq {
                            for item in c.Sources do
                                match item with
                                | FileSource(s) ->                                
                                    yield XElement(XName.Get("Source"), 
                                                   XAttribute(XName.Get("Type"), "File"), 
                                                   s)
                        })),
                    new XElement(XName.Get("OverrideTypeHandlers"),
                        Array.ofSeq(seq {
                            for item in c.OverrideTypeHandlers do
                                yield CompilerTypeHandlerConfiguration.ToXml(item)
                        })),
                    new XElement(XName.Get("OverrideSteps"),
                        Array.ofSeq(seq {
                            for item in c.OverrideSteps do
                                yield CompilerStepConfiguration.ToXml(item)
                        })),
                    new XElement(XName.Get("OverrideStepProcessors"),
                        Array.ofSeq(seq {
                            for item in c.OverrideProcessors do
                                yield CompilerStepProcessorConfiguration.ToXml(item)
                        })))
        let doc = new XDocument(el)
        doc
        
    static member internal FromXml(doc: XDocument) =
        let conf = new CompilerConfiguration()
        if(doc.Root.Attribute(XName.Get("LoadDefaultSteps")) <> null) then
            conf.LoadDefaultSteps <- bool.Parse(doc.Root.Attribute(XName.Get("LoadDefaultSteps")).Value)
        for s in doc.Root.Elements(XName.Get("Sources")) do
            for source in s.Elements(XName.Get("Source")) do
                let t = source.Attribute(XName.Get("Type")).Value
                if t = "File" then
                    conf.Sources.Add(FileSource(source.Value))
        for t in doc.Root.Elements(XName.Get("OverrideTypeHandlers")) do
            for th in t.Elements(XName.Get("CompilerTypeHandlerConfiguration")) do
                conf.OverrideTypeHandlers.Add(CompilerTypeHandlerConfiguration.FromXml(th))
        for t in doc.Root.Elements(XName.Get("OverrideSteps")) do
            for th in t.Elements(XName.Get("CompilerStepConfiguration")) do
                conf.OverrideSteps.Add(CompilerStepConfiguration.FromXml(th))
        for t in doc.Root.Elements(XName.Get("OverrideProcessors")) do
            for th in t.Elements(XName.Get("CompilerStepProcessorConfiguration")) do
                conf.OverrideProcessors.Add(CompilerStepProcessorConfiguration.FromXml(th))
        conf
                            
       
            

