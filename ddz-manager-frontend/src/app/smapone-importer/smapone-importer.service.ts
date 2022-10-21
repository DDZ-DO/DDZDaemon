import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { ImporterStatus, SmapOneImporterStatus } from './smapone-importer-status';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SmaponeImporterService {
  private baseUrl = `${environment.API_URL}/smapone/SmapOneImporter/`;

  constructor(private http: HttpClient) {

  }

  setImporterStatus(newStatus: ImporterStatus): Observable<SmapOneImporterStatus> {
    console.log("New status: "+newStatus);
    const params = new HttpParams()
      .append('newStatus',newStatus);
    return this.http.post<SmapOneImporterStatus>(this.baseUrl+"SetStatus", "", { params: params }).pipe(
      catchError(this.handleError<SmapOneImporterStatus>('getCurrentStatus'))
    );
  }

  getCurrentStatus(): Observable<SmapOneImporterStatus> {
    return this.http.get<SmapOneImporterStatus>(this.baseUrl+"GetImporterStatus").pipe(
      tap(_ => console.log('fetched status')),
      catchError(this.handleError<SmapOneImporterStatus>('getCurrentStatus'))
    );
  }

  getImporterLog(): Observable<string[]> {
    return this.http.get<string[]>(this.baseUrl+"GetImporterLog").pipe(
      tap(_ => console.log('getImporterLog')),
      catchError(this.handleError<string[]>('getImporterLog'))
    );
  }

  getProtocol(): Observable<string[]> {
    return this.http.get<string[]>(this.baseUrl+"GetControllerProtocol").pipe(
      tap(_ => console.log('getProtocol')),
      catchError(this.handleError<string[]>('getProtocol'))
    );
  }

  forceStart(): Observable<unknown> {
    return this.http.post(this.baseUrl+"ForceStart","") .pipe(
      catchError(this.handleError('addHero'))
    );
  }

  /**
    * Handle Http operation that failed.
    * Let the app continue.
    *
    * @param operation - name of the operation that failed
    * @param result - optional value to return as the observable result
    */
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {

      // TODO: send the error to remote logging infrastructure
      console.error(error); // log to console instead

      // TODO: better job of transforming error for user consumption
      console.log(`${operation} failed: ${error.message}`);

      // Let the app keep running by returning an empty result.
      return of(result as T);
    };
  }
}
