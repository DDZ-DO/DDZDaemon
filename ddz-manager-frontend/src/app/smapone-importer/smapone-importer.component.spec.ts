import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SmaponeImporterComponent } from './smapone-importer.component';

describe('SmaponeImporterComponent', () => {
  let component: SmaponeImporterComponent;
  let fixture: ComponentFixture<SmaponeImporterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SmaponeImporterComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SmaponeImporterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
