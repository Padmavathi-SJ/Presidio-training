import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TasksBulkOperationsComponent } from './tasks-bulk-operations.component';

describe('TasksBulkOperationsComponent', () => {
  let component: TasksBulkOperationsComponent;
  let fixture: ComponentFixture<TasksBulkOperationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TasksBulkOperationsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TasksBulkOperationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
