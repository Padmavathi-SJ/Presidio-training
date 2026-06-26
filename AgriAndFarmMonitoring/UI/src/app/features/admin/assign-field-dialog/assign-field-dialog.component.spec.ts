import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssignFieldDialogComponent } from './assign-field-dialog.component';

describe('AssignFieldDialogComponent', () => {
  let component: AssignFieldDialogComponent;
  let fixture: ComponentFixture<AssignFieldDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssignFieldDialogComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AssignFieldDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
