import { Component, OnInit } from '@angular/core';
import { ImporterStatus } from './smapone-importer-status';
import { SmaponeImporterService } from './smapone-importer.service';

@Component({
  selector: 'app-smapone-importer',
  templateUrl: './smapone-importer.component.html',
  styleUrls: ['./smapone-importer.component.scss']
})
export class SmaponeImporterComponent implements OnInit {
  nextExecution = "-";
  lastExecution = "-";
  status = "";
  logOutput: string[] = [];
  protocol: string[] = [];
  logDataLoading : boolean = false;
  smaponeImporterService: SmaponeImporterService;

  constructor(smaponeImporterService: SmaponeImporterService) {
    this.smaponeImporterService = smaponeImporterService;
  }

  ngOnInit(): void {
    this.ReloadStatus();
    this.ReloadLog();
  }

  StartImporter(): void {
    this.smaponeImporterService.setImporterStatus(ImporterStatus.run).subscribe(status => {
      this.status = status.status === 0? "gestoppt" : "läuft";
      this.nextExecution = new Date(status.nextExecutionTimePoint).toLocaleString()
    })
  }

  StopImporter(): void {
    this.smaponeImporterService.setImporterStatus(ImporterStatus.stop).subscribe(status => {
      this.status = status.status === 0? "gestoppt" : "läuft";
      this.nextExecution = new Date(status.nextExecutionTimePoint).toLocaleString();
      this.lastExecution = new Date(status.lastExecutionTimePoint).toLocaleString();
    })
  }

  ReloadStatus(): void {
    this.smaponeImporterService.getCurrentStatus().subscribe(status => {
      this.status = status.status === 0? "gestoppt" : "läuft";
      this.nextExecution = new Date(status.nextExecutionTimePoint).toLocaleString();
      this.lastExecution = new Date(status.lastExecutionTimePoint).toLocaleString();
    });
  }

  ReloadLog(): void {
    this.logDataLoading = true;
    this.smaponeImporterService.getImporterLog().subscribe( logLines =>{
      this.logOutput = logLines;
      console.log(this.logOutput);
      this.logDataLoading = false;
    });
  }

  ReloadProtocol(): void {
    this.smaponeImporterService.getProtocol().subscribe( protcolLines =>{
      this.protocol = protcolLines;
      console.log(this.logOutput);
    });
  }

  ForceStart(): void{
    this.smaponeImporterService.forceStart().subscribe(()=>{
      this.ReloadStatus();
    })
  }
}
