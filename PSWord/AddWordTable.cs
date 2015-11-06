﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Novacode;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace PSWord
{
    using System.Data.Odbc;

    [Cmdlet(VerbsCommon.Add, "WordTable")]
    public class AddWordTable : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string FilePath { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true)]
        public Object[] InputObject { get; set; }

        [Parameter]
        public TableDesign Design { get; set; }

        [Parameter]
        public SwitchParameter Show { get; set; }

        protected override void BeginProcessing()
        {
            var fullPath = Path.GetFullPath(this.FilePath);
            if (!File.Exists(fullPath))
            {
                var createDoc = DocX.Create(fullPath);
                createDoc.Save();
                createDoc.Dispose();
            }
        }

        protected override void ProcessRecord()
        {
            ProviderInfo providerInfo = null;
            var resolvedFile = this.GetResolvedProviderPathFromPSPath(this.FilePath, out providerInfo);
           
            using (DocX document = DocX.Load(resolvedFile[0]))
            {
                var header = this.InputObject[0].GetType().GetProperties().Select(p => p.Name).ToArray();
                var docTable = document.AddTable(1, header.Count());
                int contagem = 0;
                try
                {
                    if (contagem == 0)
                    {
                        contagem++;
                       var row = 0;
                        var Column = 0;

                        foreach (var name in header)
                        {
                            WriteObject(name);
                            docTable.Rows[row].Cells[Column++].Paragraphs[0].Append(name);
                        }
                    }

                    var ColumnIndex = 0;
                    var newRow = docTable.InsertRow();

                    foreach (var name in header)
                    {
                        var appendData = this.InputObject[0].GetType().GetProperty(name).GetValue(this.InputObject[0], null);
                        
                        newRow.Cells[ColumnIndex++].Paragraphs[0].Append((string)appendData);
                    }
                }
                catch (Exception exception)
                {
                    //this.WriteError(new ErrorRecord(exception, exception.HResult.ToString(), ErrorCategory.WriteError, exception));
                }
                finally
                {
                    var paragraph = document.InsertParagraph();
                    paragraph.InsertTableAfterSelf(docTable);
                    docTable.Design = this.Design;

                    document.Save();

                    if (this.Show.IsPresent)
                    {
                        Process.Start(resolvedFile[0]);
                    }
                }
            }
        }
    }
}

