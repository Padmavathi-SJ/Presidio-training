import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkerFields } from './worker-fields.component';

describe('WorkerFields', () => {
  let component: WorkerFields;
  let fixture: ComponentFixture<WorkerFields>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WorkerFields],
    }).compileComponents();

    fixture = TestBed.createComponent(WorkerFields);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
