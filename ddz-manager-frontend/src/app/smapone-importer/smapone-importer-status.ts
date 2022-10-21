
export enum ImporterStatus {
   stop = 0,
   run = 1
}

export interface SmapOneImporterStatus {
  status: ImporterStatus;
  nextExecutionTimePoint: number;
  lastExecutionTimePoint: number;
}
