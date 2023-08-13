import { Component, ElementRef, NgZone, OnInit, Renderer2, ViewChild } from '@angular/core';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {

  @ViewChild('time')
  public time!: ElementRef;

  @ViewChild('date')
  public date!: ElementRef;

  currentPageTitle: string = "SmapOneImporter";

  constructor(private zone: NgZone, private renderer: Renderer2) {
    this.zone.runOutsideAngular(() => {
      setInterval(() => {
        this.renderer.setProperty(this.time.nativeElement, 'textContent', new Date().toLocaleTimeString());
        this.renderer.setProperty(this.date.nativeElement, 'textContent', new Date().toLocaleDateString());
      }, 1);
    });
  }



}
