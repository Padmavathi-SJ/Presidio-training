import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AlertDetailDialogComponent } from './alert-detail-dialog.component';

describe('AlertDetailDialogComponent', () => {
  let component: AlertDetailDialogComponent;
  let fixture: ComponentFixture<AlertDetailDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlertDetailDialogComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AlertDetailDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
